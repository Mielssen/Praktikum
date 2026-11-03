// k6 стрес-тест ендпоінтів Favorite: GET/POST/DELETE /api/favorites
//
// Параметри (через -e):
//   PERF_EMAIL, PERF_PASSWORD  - тестовий користувач (обов'язково)
//   FIRST_TOUR_ID              - id першого "PERF_TOUR_" туру (з perf_seed.sql) (обов'язково)
//   LOAD                       - small | medium | large   (5 / 50 / 200 VU)
//   FAV_COUNT                  - скільки турів заздалегідь у обраному: 10 | 100 | 1000
//   BASE_URL                   - за замовчуванням http://localhost:5265
//   POOL_SIZE                  - скільки турів створено seed-скриптом (за замовчуванням 1300)

import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter } from 'k6/metrics';
import { textSummary } from 'https://jslib.k6.io/k6-summary/0.0.2/index.js';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5265';
const EMAIL = __ENV.PERF_EMAIL;
const PASSWORD = __ENV.PERF_PASSWORD;
const FIRST_TOUR_ID = parseInt(__ENV.FIRST_TOUR_ID || '0', 10);
const FAV_COUNT = parseInt(__ENV.FAV_COUNT || '10', 10);
const POOL_SIZE = parseInt(__ENV.POOL_SIZE || '1300', 10);
const LOAD = (__ENV.LOAD || 'small').toLowerCase();

const PROFILES = { small: 5, medium: 50, large: 200 };
const VUS = PROFILES[LOAD] || PROFILES.small;

const GET_TAG = 'GET /api/favorites';
const POST_TAG = 'POST /api/favorites';
const DEL_TAG = 'DELETE /api/favorites/{id}';

// 200 і 400 (ALREADY_EXISTS / NOT_FOUND) - очікувані бізнес-відповіді.
// 401, 429, 5xx - помилки.
http.setResponseCallback(http.expectedStatuses(200, 400));

const businessConflicts = new Counter('business_conflicts');

export const options = {
  scenarios: {
    favorites: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: [
        { duration: '10s', target: VUS },
        { duration: '40s', target: VUS },
        { duration: '10s', target: 0 },
      ],
      gracefulRampDown: '10s',
    },
  },
  setupTimeout: '600s',
  teardownTimeout: '600s',
  summaryTrendStats: ['avg', 'med', 'p(90)', 'p(95)', 'p(99)', 'max'],
  thresholds: {
    http_req_failed: ['rate<0.01'],
    [`http_req_duration{name:${GET_TAG}}`]: ['p(95)<500'],
    [`http_req_duration{name:${POST_TAG}}`]: ['p(95)<500'],
    [`http_req_duration{name:${DEL_TAG}}`]: ['p(95)<500'],
  },
};

function authHeaders(token) {
  return { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` };
}

export function setup() {
  if (!EMAIL || !PASSWORD || !FIRST_TOUR_ID) {
    throw new Error('Потрібні PERF_EMAIL, PERF_PASSWORD, FIRST_TOUR_ID');
  }
  if (FAV_COUNT + VUS > POOL_SIZE) {
    throw new Error(`Замало турів: FAV_COUNT (${FAV_COUNT}) + VU (${VUS}) > POOL_SIZE (${POOL_SIZE})`);
  }

  const login = http.post(
    `${BASE_URL}/api/auth/login`,
    JSON.stringify({ email: EMAIL, password: PASSWORD }),
    { headers: { 'Content-Type': 'application/json' }, tags: { name: 'setup' } },
  );
  if (login.status !== 200) {
    throw new Error(`Login не вдався: ${login.status} ${login.body}`);
  }
  const token = login.json('token');
  const headers = authHeaders(token);

  // Заповнюємо обране користувача до потрібного розміру (мале / середнє / велике).
  for (let i = 0; i < FAV_COUNT; i++) {
    const r = http.post(
      `${BASE_URL}/api/favorites`,
      JSON.stringify({ tourId: FIRST_TOUR_ID + i }),
      { headers, tags: { name: 'setup' } },
    );
    if (r.status !== 200 && r.status !== 400) {
      throw new Error(`Не вдалося додати тур ${FIRST_TOUR_ID + i}: ${r.status} ${r.body}`);
    }
  }

  return { token };
}

export default function (data) {
  const headers = authHeaders(data.token);
  // Кожен VU працює зі своїм туром, щоб VU не конфліктували між собою.
  const tourId = FIRST_TOUR_ID + FAV_COUNT + (__VU - 1);

  const list = http.get(`${BASE_URL}/api/favorites`, { headers, tags: { name: GET_TAG } });
  check(list, { 'GET 200': (r) => r.status === 200 });

  const add = http.post(
    `${BASE_URL}/api/favorites`,
    JSON.stringify({ tourId }),
    { headers, tags: { name: POST_TAG } },
  );
  check(add, { 'POST 200/400': (r) => r.status === 200 || r.status === 400 });
  if (add.status === 400) businessConflicts.add(1);

  const del = http.del(`${BASE_URL}/api/favorites/${tourId}`, null, {
    headers,
    tags: { name: DEL_TAG },
  });
  check(del, { 'DELETE 200/400': (r) => r.status === 200 || r.status === 400 });

  sleep(0.2);
}

export function teardown(data) {
  // Прибираємо за собою: видаляємо заздалегідь додані обрані.
  const headers = authHeaders(data.token);
  for (let i = 0; i < FAV_COUNT; i++) {
    http.del(`${BASE_URL}/api/favorites/${FIRST_TOUR_ID + i}`, null, {
      headers,
      tags: { name: 'teardown' },
    });
  }
}

function trend(data, tag) {
  const m = data.metrics[`http_req_duration{name:${tag}}`];
  const v = m ? m.values : {};
  const pick = (k) => (v[k] === undefined ? null : v[k]);
  return { avg: pick('avg'), med: pick('med'), p90: pick('p(90)'), p95: pick('p(95)'), p99: pick('p(99)'), max: pick('max') };
}

export function handleSummary(data) {
  const failed = data.metrics.http_req_failed ? data.metrics.http_req_failed.values.rate : null;
  const iter = data.metrics.iterations ? data.metrics.iterations.values : {};
  const compact = {
    load: LOAD,
    vus: VUS,
    favCount: FAV_COUNT,
    iterations: iter.count || 0,
    iterationsPerSec: iter.rate || 0,
    failedRate: failed,
    endpoints: {
      get: trend(data, GET_TAG),
      post: trend(data, POST_TAG),
      delete: trend(data, DEL_TAG),
    },
  };
  return {
    stdout: textSummary(data, { indent: ' ', enableColors: true }),
    [`results/k6_${LOAD}_fav${FAV_COUNT}.json`]: JSON.stringify(compact, null, 2),
  };
}
