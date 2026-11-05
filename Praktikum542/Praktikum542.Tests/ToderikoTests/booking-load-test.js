
import http from 'k6/http';
import { check, sleep } from 'k6';

const vus = Number(__ENV.VUS || 5);
const duration = __ENV.DURATION || '10s';

export const options = {
    vus: vus,
    duration: duration,
    thresholds: {
        http_req_failed: ['rate<0.05'],
        http_req_duration: ['p(95)<1000'],
        checks: ['rate>0.95'],
    },
};

export default function () {
    const response = http.get('https://localhost:7227/api/tours');

    check(response, {
        'status is 200': (r) => r.status === 200,
    });

    sleep(1);
}
