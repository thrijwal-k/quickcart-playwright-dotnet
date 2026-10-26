import React, { useEffect, useMemo, useState } from 'react';

const UK_POSTCODE = /^[A-Z]{1,2}\d[A-Z\d]? ?\d[A-Z]{2}$/i;
const EMAIL = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const money = (n) => `£${n.toFixed(2)}`;

function Login({ onLogin }) {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');

  async function submit(e) {
    e.preventDefault();
    setError('');
    if (!username || !password) return setError('Username and password are required');
    const res = await fetch('/api/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password }),
    });
    const body = await res.json();
    if (!res.ok) return setError(body.error);
    onLogin(body.name);
  }

  return (
    <form className="card" onSubmit={submit} aria-label="Login form">
      <h1>Sign in to QuickCart</h1>
      <label htmlFor="username">Username</label>
      <input id="username" data-testid="username" value={username} onChange={(e) => setUsername(e.target.value)} />
      <label htmlFor="password">Password</label>
      <input id="password" data-testid="password" type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
      {error && <p className="error" role="alert" data-testid="login-error">{error}</p>}
      <button className="primary" type="submit" data-testid="login-button">Sign in</button>
    </form>
  );
}

function Products({ cart, onAdd }) {
  const [products, setProducts] = useState([]);
  const [query, setQuery] = useState('');

  useEffect(() => {
    const controller = new AbortController();
    fetch(`/api/products?q=${encodeURIComponent(query)}`, { signal: controller.signal })
      .then((r) => r.json())
      .then(setProducts)
      .catch(() => {});
    return () => controller.abort();
  }, [query]);

  return (
    <section>
      <h1>Products</h1>
      <label htmlFor="search">Search products</label>
      <input id="search" data-testid="search" value={query} onChange={(e) => setQuery(e.target.value)} placeholder="e.g. watch" />
      {products.length === 0 && <p data-testid="no-results">No products match your search</p>}
      <div className="grid" style={{ marginTop: 16 }}>
        {products.map((p) => {
          const inCart = cart[p.id] ?? 0;
          const soldOut = p.stock === 0;
          return (
            <article className="card" key={p.id} data-testid="product-card" data-product={p.name}>
              <h2 style={{ fontSize: 18 }}>{p.name}</h2>
              <p>{p.category}</p>
              <p data-testid="product-price">{money(p.price)}</p>
              <button
                className="primary"
                data-testid="add-to-cart"
                disabled={soldOut || inCart >= p.stock}
                onClick={() => onAdd(p)}
              >
                {soldOut ? 'Sold out' : 'Add to cart'}
              </button>
            </article>
          );
        })}
      </div>
    </section>
  );
}

function Cart({ cart, products, onChange, onCheckout }) {
  const lines = Object.entries(cart).map(([id, qty]) => ({ product: products[id], qty }));
  const subtotal = lines.reduce((s, l) => s + l.product.price * l.qty, 0);
  if (lines.length === 0) return <section className="card"><h1>Your cart</h1><p data-testid="empty-cart">Your cart is empty</p></section>;
  return (
    <section className="card">
      <h1>Your cart</h1>
      <table>
        <thead><tr><th>Item</th><th>Qty</th><th>Price</th><th></th></tr></thead>
        <tbody>
          {lines.map(({ product, qty }) => (
            <tr key={product.id} data-testid="cart-line" data-product={product.name}>
              <td>{product.name}</td>
              <td>
                <button aria-label={`Decrease ${product.name}`} onClick={() => onChange(product, qty - 1)}>-</button>
                <span data-testid="line-qty" style={{ margin: '0 8px' }}>{qty}</span>
                <button aria-label={`Increase ${product.name}`} disabled={qty >= product.stock} onClick={() => onChange(product, qty + 1)}>+</button>
              </td>
              <td>{money(product.price * qty)}</td>
              <td><button data-testid="remove-line" onClick={() => onChange(product, 0)}>Remove</button></td>
            </tr>
          ))}
        </tbody>
      </table>
      <p>Subtotal: <strong data-testid="subtotal">{money(subtotal)}</strong></p>
      <button className="primary" data-testid="checkout" onClick={onCheckout}>Checkout</button>
    </section>
  );
}

function Checkout({ cart, onPlaced }) {
  const [form, setForm] = useState({ name: '', email: '', postcode: '' });
  const [errors, setErrors] = useState([]);
  const set = (k) => (e) => setForm({ ...form, [k]: e.target.value });

  async function submit(e) {
    e.preventDefault();
    const local = [];
    if (!form.name.trim()) local.push('Name is required');
    if (!EMAIL.test(form.email)) local.push('A valid email is required');
    if (!UK_POSTCODE.test(form.postcode)) local.push('A valid UK postcode is required');
    setErrors(local);
    if (local.length) return;
    const items = Object.entries(cart).map(([id, quantity]) => ({ productId: Number(id), quantity }));
    const res = await fetch('/api/orders', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ items, customer: form }),
    });
    const body = await res.json();
    if (!res.ok) return setErrors(body.errors);
    onPlaced(body);
  }

  return (
    <form className="card" onSubmit={submit} aria-label="Checkout form" noValidate>
      <h1>Checkout</h1>
      <label htmlFor="name">Full name</label>
      <input id="name" data-testid="name" value={form.name} onChange={set('name')} />
      <label htmlFor="email">Email</label>
      <input id="email" data-testid="email" value={form.email} onChange={set('email')} />
      <label htmlFor="postcode">Postcode</label>
      <input id="postcode" data-testid="postcode" value={form.postcode} onChange={set('postcode')} />
      {errors.map((err) => <p className="error" role="alert" key={err} data-testid="checkout-error">{err}</p>)}
      <button className="primary" type="submit" data-testid="place-order">Place order</button>
    </form>
  );
}

export default function App() {
  const [user, setUser] = useState(null);
  const [view, setView] = useState('products');
  const [cart, setCart] = useState({});
  const [known, setKnown] = useState({});
  const [order, setOrder] = useState(null);
  const count = useMemo(() => Object.values(cart).reduce((a, b) => a + b, 0), [cart]);

  const change = (product, qty) => {
    setKnown((k) => ({ ...k, [product.id]: product }));
    setCart((c) => {
      const next = { ...c };
      if (qty <= 0) delete next[product.id];
      else next[product.id] = qty;
      return next;
    });
  };

  if (!user) return <main><Login onLogin={(name) => { setUser(name); setView('products'); }} /></main>;

  return (
    <>
      <header>
        <strong>QuickCart</strong>
        <span data-testid="welcome">Hello, {user}</span>
        <nav style={{ display: 'flex', gap: 8 }}>
          <button onClick={() => setView('products')} data-testid="nav-products">Products</button>
          <button onClick={() => setView('cart')} data-testid="nav-cart">Cart (<span data-testid="cart-count">{count}</span>)</button>
          <button onClick={() => { setUser(null); setCart({}); }} data-testid="logout">Sign out</button>
        </nav>
      </header>
      <main>
        {view === 'products' && <Products cart={cart} onAdd={(p) => change(p, (cart[p.id] ?? 0) + 1)} />}
        {view === 'cart' && <Cart cart={cart} products={known} onChange={change} onCheckout={() => setView('checkout')} />}
        {view === 'checkout' && <Checkout cart={cart} onPlaced={(o) => { setOrder(o); setCart({}); setView('done'); }} />}
        {view === 'done' && order && (
          <section className="card">
            <h1 className="success" data-testid="confirmation">Order confirmed</h1>
            <p>Order number <strong data-testid="order-id">{order.id}</strong></p>
            <p>Total paid <strong data-testid="order-total">{money(order.total)}</strong></p>
          </section>
        )}
      </main>
    </>
  );
}
