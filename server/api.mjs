// QuickCart demo API: a tiny dependency-free REST API that the React app and the API tests use.
import http from 'node:http';

const PORT = Number(process.env.API_PORT ?? 3001);

const USERS = {
  shopper: { password: 'Passw0rd!', locked: false, name: 'Sam Shopper' },
  locked_user: { password: 'Passw0rd!', locked: true, name: 'Lee Locked' },
};

const PRODUCTS = [
  { id: 1, name: 'Wireless Headphones', category: 'Audio', price: 59.99, stock: 12 },
  { id: 2, name: 'Bluetooth Speaker', category: 'Audio', price: 34.5, stock: 8 },
  { id: 3, name: 'Smart Watch', category: 'Wearables', price: 129.0, stock: 5 },
  { id: 4, name: 'Fitness Band', category: 'Wearables', price: 39.99, stock: 0 },
  { id: 5, name: 'USB-C Charger', category: 'Accessories', price: 19.99, stock: 30 },
  { id: 6, name: 'Laptop Stand', category: 'Accessories', price: 27.75, stock: 14 },
];

const UK_POSTCODE = /^[A-Z]{1,2}\d[A-Z\d]? ?\d[A-Z]{2}$/i;
const EMAIL = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

let orders = new Map();
let nextOrder = 1001;
const tokens = new Map();

function send(res, status, body) {
  res.writeHead(status, { 'Content-Type': 'application/json' });
  res.end(JSON.stringify(body));
}

function readJson(req) {
  return new Promise((resolve) => {
    let raw = '';
    req.on('data', (chunk) => (raw += chunk));
    req.on('end', () => {
      try {
        resolve(raw ? JSON.parse(raw) : {});
      } catch {
        resolve(null);
      }
    });
  });
}

export function validateOrder(body) {
  const errors = [];
  if (!body || !Array.isArray(body.items) || body.items.length === 0) errors.push('Order must contain at least one item');
  for (const item of body?.items ?? []) {
    const product = PRODUCTS.find((p) => p.id === item.productId);
    if (!product) errors.push(`Unknown product ${item.productId}`);
    else if (!Number.isInteger(item.quantity) || item.quantity < 1) errors.push(`Invalid quantity for ${product.name}`);
    else if (item.quantity > product.stock) errors.push(`Only ${product.stock} of ${product.name} in stock`);
  }
  const c = body?.customer ?? {};
  if (!c.name || !String(c.name).trim()) errors.push('Name is required');
  if (!EMAIL.test(c.email ?? '')) errors.push('A valid email is required');
  if (!UK_POSTCODE.test(c.postcode ?? '')) errors.push('A valid UK postcode is required');
  return errors;
}

const server = http.createServer(async (req, res) => {
  const url = new URL(req.url, `http://localhost:${PORT}`);
  const path = url.pathname;

  if (req.method === 'GET' && path === '/api/health') return send(res, 200, { status: 'ok' });

  if (req.method === 'POST' && path === '/api/login') {
    const body = await readJson(req);
    const user = USERS[body?.username];
    if (!user || user.password !== body?.password) return send(res, 401, { error: 'Username or password is incorrect' });
    if (user.locked) return send(res, 403, { error: 'This account is locked' });
    const token = `token-${body.username}-${Date.now()}`;
    tokens.set(token, body.username);
    return send(res, 200, { token, name: user.name });
  }

  if (req.method === 'GET' && path === '/api/products') {
    const q = (url.searchParams.get('q') ?? '').toLowerCase();
    return send(res, 200, PRODUCTS.filter((p) => p.name.toLowerCase().includes(q)));
  }

  const productMatch = path.match(/^\/api\/products\/(\d+)$/);
  if (req.method === 'GET' && productMatch) {
    const product = PRODUCTS.find((p) => p.id === Number(productMatch[1]));
    return product ? send(res, 200, product) : send(res, 404, { error: 'Product not found' });
  }

  if (req.method === 'POST' && path === '/api/orders') {
    const body = await readJson(req);
    if (body === null) return send(res, 400, { errors: ['Body must be valid JSON'] });
    const errors = validateOrder(body);
    if (errors.length) return send(res, 422, { errors });
    const total = body.items.reduce((sum, i) => sum + PRODUCTS.find((p) => p.id === i.productId).price * i.quantity, 0);
    const order = { id: nextOrder++, items: body.items, customer: body.customer, total: Math.round(total * 100) / 100, status: 'CONFIRMED' };
    orders.set(order.id, order);
    return send(res, 201, order);
  }

  const orderMatch = path.match(/^\/api\/orders\/(\d+)$/);
  if (req.method === 'GET' && orderMatch) {
    const order = orders.get(Number(orderMatch[1]));
    return order ? send(res, 200, order) : send(res, 404, { error: 'Order not found' });
  }

  if (req.method === 'POST' && path === '/api/reset') {
    orders = new Map();
    nextOrder = 1001;
    return send(res, 200, { status: 'reset' });
  }

  return send(res, 404, { error: 'Not found' });
});

server.listen(PORT, () => console.log(`QuickCart API listening on http://localhost:${PORT}`));
