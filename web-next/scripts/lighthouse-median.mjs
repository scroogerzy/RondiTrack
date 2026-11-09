import { mkdir, readFile } from 'node:fs/promises';
import { spawnSync } from 'node:child_process';
import { resolve } from 'node:path';

const target = process.argv[2];
if (!target) {
  console.error('Usage: npm run performance:lighthouse -- http://127.0.0.1:3000/login');
  process.exit(2);
}

const outputDir = resolve('artifacts/lighthouse');
await mkdir(outputDir, { recursive: true });
const reports = [];

for (let run = 1; run <= 3; run += 1) {
  const outputPath = resolve(outputDir, `mobile-run-${run}.json`);
  console.log(`Lighthouse mobile run ${run}/3: ${target}`);
  const result = spawnSync('npx', [
    '--yes', 'lighthouse@12', target,
    '--only-categories=performance',
    '--form-factor=mobile',
    '--screenEmulation.mobile',
    '--throttling-method=simulate',
    '--throttling.cpuSlowdownMultiplier=4',
    '--output=json',
    `--output-path=${outputPath}`,
    '--chrome-flags=--headless --no-sandbox',
    '--quiet'
  ], { stdio: 'inherit', shell: process.platform === 'win32' });

  if (result.status !== 0) {
    console.error(`Lighthouse run ${run} failed; no median has been produced.`);
    process.exit(result.status ?? 1);
  }
  reports.push(JSON.parse(await readFile(outputPath, 'utf8')));
}

const median = values => [...values].sort((a, b) => a - b)[Math.floor(values.length / 2)];
const lcp = reports.map(report => report.audits['largest-contentful-paint'].numericValue);
const cls = reports.map(report => report.audits['cumulative-layout-shift'].numericValue);
const scores = reports.map(report => report.categories.performance.score * 100);
const summary = {
  target,
  formFactor: 'mobile',
  throttling: 'Lighthouse simulated mobile, 4x CPU slowdown',
  runs: reports.length,
  median: {
    performanceScore: Number(median(scores).toFixed(1)),
    lcpMs: Math.round(median(lcp)),
    cls: Number(median(cls).toFixed(3))
  },
  targets: { lcpMs: 2500, cls: 0.1 },
  sourceReports: ['mobile-run-1.json', 'mobile-run-2.json', 'mobile-run-3.json']
};
await import('node:fs/promises').then(({ writeFile }) =>
  writeFile(resolve(outputDir, 'median-summary.json'), `${JSON.stringify(summary, null, 2)}\n`)
);
console.log(JSON.stringify(summary, null, 2));
