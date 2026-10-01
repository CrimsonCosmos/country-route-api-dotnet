import { useState } from 'react';

export default function App() {
  const [code, setCode] = useState('');
  const [result, setResult] = useState(null); // { destination, list } | { error }
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e) {
    e.preventDefault();
    const trimmed = code.trim();
    if (!trimmed) return;
    setLoading(true);
    try {
      const res = await fetch(`/api/${encodeURIComponent(trimmed)}`);
      setResult(await res.json());
    } catch {
      setResult({ error: 'Could not reach the server. Please try again.' });
    } finally {
      setLoading(false);
    }
  }

  return (
    <main>
      <h1>Country Route</h1>
      <p className="hint">
        Enter a three-letter North American country code to see the countries a driver
        crosses going from the USA.
      </p>

      <form onSubmit={handleSubmit}>
        <label htmlFor="code">Country code</label>
        <input
          id="code"
          value={code}
          onChange={(e) => setCode(e.target.value.toUpperCase())}
          placeholder="PAN"
          maxLength={3}
          autoComplete="off"
          autoFocus
        />
        <button type="submit" disabled={loading || !code.trim()}>
          {loading ? 'Looking up…' : 'Find route'}
        </button>
      </form>

      {result?.error && <p role="alert" className="error">{result.error}</p>}
      {result?.list && (
        <section aria-live="polite">
          <h2>Route to {result.destination}</h2>
          <ol>
            {result.list.map((c) => (
              <li key={c}>{c}</li>
            ))}
          </ol>
        </section>
      )}
    </main>
  );
}
