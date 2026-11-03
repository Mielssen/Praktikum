import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '5s', target: 10 },
    { duration: '10s', target: 30 },
    { duration: '5s', target: 0 },
  ],
};

export default function () {
  const res = http.get('http://localhost:5265/api/reviews/tour/1/average');
  check(res, {
    'status is 200': (r) => r.status === 200,
  });
  sleep(1);
}