// Dev server (stands in for `vite`): serves src/ + public/ + vendor scripts,
// proxies /api/* to the backend, and falls back to index.html for client-side
// routes. Tailwind runs in watch mode alongside it.
//
//   npm run dev                       http://localhost:5173, API at http://localhost:9000
//   PORT=8080 API_TARGET=http://localhost:5000 npm run dev
//   npm run preview                   serve dist/ (after npm run build) instead
import { spawn } from 'node:child_process';
import { createReadStream, existsSync, mkdirSync, statSync } from 'node:fs';
import { createServer, request } from 'node:http';
import { extname, join, normalize, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { VENDOR_FILES } from './vendor.mjs';

const root = resolve(fileURLToPath(import.meta.url), '../..');
const port = Number(process.env.PORT || 5173);
const apiTarget = new URL(process.env.API_TARGET || 'http://localhost:9000');
const serveDist = process.argv.includes('--dist');

const MIME = {
  '.html': 'text/html; charset=utf-8',
  '.js': 'text/javascript; charset=utf-8',
  '.css': 'text/css; charset=utf-8',
  '.json': 'application/json',
  '.png': 'image/png',
  '.jpg': 'image/jpeg',
  '.jpeg': 'image/jpeg',
  '.webp': 'image/webp',
  '.svg': 'image/svg+xml',
  '.ico': 'image/x-icon',
};

// Directories searched, in order, for a static file.
let staticRoots;
if (serveDist) {
  staticRoots = [join(root, 'dist')];
} else {
  const cssDir = join(root, '.dev');
  mkdirSync(cssDir, { recursive: true });
  staticRoots = [cssDir, join(root, 'src'), join(root, 'public')];

  const tailwind = spawn(join(root, 'node_modules/.bin/tailwindcss'),
    ['-i', join(root, 'src/styles.css'), '-o', join(cssDir, 'app.css'), '--watch=always'],
    { cwd: root, stdio: 'inherit' });
  process.on('exit', () => tailwind.kill());
  process.on('SIGINT', () => process.exit(0));
  process.on('SIGTERM', () => process.exit(0));
}

function resolveStatic(urlPath) {
  if (!serveDist && urlPath.startsWith('/vendor/')) {
    const from = VENDOR_FILES[urlPath.slice('/vendor/'.length)];
    return from ? join(root, from) : null;
  }
  for (const dir of staticRoots) {
    const file = normalize(join(dir, urlPath));
    if (file.startsWith(dir) && existsSync(file) && statSync(file).isFile()) return file;
  }
  return null;
}

function sendFile(res, file, status = 200) {
  res.writeHead(status, {
    'Content-Type': MIME[extname(file)] || 'application/octet-stream',
    'Cache-Control': 'no-store',
  });
  createReadStream(file).pipe(res);
}

function proxy(req, res) {
  const upstream = request({
    protocol: apiTarget.protocol,
    hostname: apiTarget.hostname,
    port: apiTarget.port,
    method: req.method,
    path: req.url,
    headers: { ...req.headers, host: apiTarget.host },
  }, upRes => {
    res.writeHead(upRes.statusCode, upRes.headers);
    upRes.pipe(res);
  });
  upstream.on('error', err => {
    res.writeHead(502, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify({ error: `API proxy error: ${err.code || err.message}` }));
  });
  req.pipe(upstream);
}

createServer((req, res) => {
  const urlPath = decodeURIComponent(new URL(req.url, 'http://localhost').pathname);

  if (urlPath.startsWith('/api/')) return proxy(req, res);

  const file = resolveStatic(urlPath);
  if (file) return sendFile(res, file);

  // Missing assets are real 404s; anything else is a client-side route.
  if (extname(urlPath)) {
    res.writeHead(404);
    return res.end('Not found');
  }
  sendFile(res, join(staticRoots[serveDist ? 0 : 1], 'index.html'));
}).listen(port, () => {
  console.log(`QuillingArtist UI on http://localhost:${port}  (api -> ${apiTarget.origin}${serveDist ? ', serving dist/' : ''})`);
});
