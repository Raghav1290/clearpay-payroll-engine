import { useEffect, useState } from 'react'
import './index.css'
import { api, type PayBreakdown, type PayRunSummary, type TimesheetEntry } from './api'

interface Row {
  date: string
  hours: string
  isPublicHoliday: boolean
}

function emptyRow(): Row {
  return { date: '', hours: '', isPublicHoliday: false }
}

function formatMoney(value: number): string {
  return value.toLocaleString('en-NZ', { style: 'currency', currency: 'NZD' })
}

function App() {
  const [employeeName, setEmployeeName] = useState('')
  const [baseHourlyRate, setBaseHourlyRate] = useState('')
  const [rows, setRows] = useState<Row[]>([emptyRow()])
  const [result, setResult] = useState<PayBreakdown | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)
  const [history, setHistory] = useState<PayRunSummary[]>([])

  const loadHistory = () => api.listRuns().then(setHistory).catch(() => undefined)

  useEffect(() => {
    loadHistory()
  }, [])

  function updateRow(index: number, patch: Partial<Row>) {
    setRows((prev) => prev.map((row, i) => (i === index ? { ...row, ...patch } : row)))
  }

  function addRow() {
    setRows((prev) => [...prev, emptyRow()])
  }

  function removeRow(index: number) {
    setRows((prev) => (prev.length > 1 ? prev.filter((_, i) => i !== index) : prev))
  }

  async function submit() {
    setError(null)

    const rate = Number(baseHourlyRate)
    if (!employeeName.trim()) {
      setError('Enter an employee name.')
      return
    }
    if (!rate || rate <= 0) {
      setError('Enter a base hourly rate greater than zero.')
      return
    }

    const entries: TimesheetEntry[] = []
    for (const row of rows) {
      if (!row.date || !row.hours) continue
      entries.push({ date: row.date, hoursWorked: Number(row.hours), isPublicHoliday: row.isPublicHoliday })
    }
    if (entries.length === 0) {
      setError('Add at least one timesheet row with a date and hours.')
      return
    }

    setSubmitting(true)
    try {
      const breakdown = await api.calculate(employeeName.trim(), rate, entries)
      setResult(breakdown)
      loadHistory()
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Something went wrong')
    } finally {
      setSubmitting(false)
    }
  }

  async function viewRun(id: number) {
    setError(null)
    try {
      setResult(await api.getRun(id))
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Could not load that pay run')
    }
  }

  return (
    <div className="page">
      <header className="header">
        <h1>ClearPay</h1>
        <p>A small timesheet and payroll calculation engine, built to work through ordinary, overtime and public holiday pay rules.</p>
      </header>

      <main className="layout">
        <section className="panel">
          <h2>New pay run</h2>

          <div className="field-row">
            <label>
              Employee name
              <input value={employeeName} onChange={(e) => setEmployeeName(e.target.value)} placeholder="Jamie Lee" />
            </label>
            <label>
              Base hourly rate ($)
              <input
                type="number"
                min="0"
                step="0.5"
                value={baseHourlyRate}
                onChange={(e) => setBaseHourlyRate(e.target.value)}
                placeholder="28.50"
              />
            </label>
          </div>

          <table className="timesheet">
            <thead>
              <tr>
                <th>Date</th>
                <th>Hours worked</th>
                <th>Public holiday</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {rows.map((row, i) => (
                <tr key={i}>
                  <td>
                    <input type="date" value={row.date} onChange={(e) => updateRow(i, { date: e.target.value })} />
                  </td>
                  <td>
                    <input
                      type="number"
                      min="0"
                      max="24"
                      step="0.25"
                      value={row.hours}
                      onChange={(e) => updateRow(i, { hours: e.target.value })}
                    />
                  </td>
                  <td className="center">
                    <input
                      type="checkbox"
                      checked={row.isPublicHoliday}
                      onChange={(e) => updateRow(i, { isPublicHoliday: e.target.checked })}
                    />
                  </td>
                  <td>
                    <button type="button" className="link-button" onClick={() => removeRow(i)}>
                      Remove
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          <button type="button" className="secondary" onClick={addRow}>
            Add day
          </button>

          {error && <p className="error">{error}</p>}

          <button type="button" className="primary" onClick={submit} disabled={submitting}>
            {submitting ? 'Calculating…' : 'Calculate pay'}
          </button>
        </section>

        <section className="panel">
          <h2>Breakdown</h2>
          {!result ? (
            <p className="muted">Run a calculation to see the pay breakdown here.</p>
          ) : (
            <div>
              <p className="result-heading">
                {result.employeeName} <span className="muted">at {formatMoney(result.baseHourlyRate)}/hr</span>
              </p>

              <div className="summary-grid">
                <div>
                  <span className="muted">Ordinary</span>
                  <strong>{result.ordinaryHours}h</strong>
                  <span>{formatMoney(result.ordinaryPay)}</span>
                </div>
                <div>
                  <span className="muted">Overtime</span>
                  <strong>{result.overtimeHours}h</strong>
                  <span>{formatMoney(result.overtimePay)}</span>
                </div>
                <div>
                  <span className="muted">Public holiday</span>
                  <strong>{result.publicHolidayHours}h</strong>
                  <span>{formatMoney(result.publicHolidayPay)}</span>
                </div>
              </div>

              <p className="total">
                Total: {result.totalHours}h &middot; {formatMoney(result.totalPay)}
              </p>

              <table className="lines">
                <thead>
                  <tr>
                    <th>Date</th>
                    <th>Rule</th>
                    <th>Hours</th>
                    <th>Rate</th>
                    <th>Pay</th>
                  </tr>
                </thead>
                <tbody>
                  {result.lines.map((line, i) => (
                    <tr key={i}>
                      <td>{line.date}</td>
                      <td>{line.ruleName}</td>
                      <td>{line.hours}</td>
                      <td>{formatMoney(line.rate)}</td>
                      <td>{formatMoney(line.pay)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          <h2 className="history-heading">Recent pay runs</h2>
          {history.length === 0 ? (
            <p className="muted">No pay runs saved yet.</p>
          ) : (
            <ul className="history-list">
              {history.map((h) => (
                <li key={h.id}>
                  <button type="button" className="link-button" onClick={() => viewRun(h.id)}>
                    {h.employeeName} &middot; {h.totalHours}h &middot; {formatMoney(h.totalPay)}
                  </button>
                </li>
              ))}
            </ul>
          )}
        </section>
      </main>
    </div>
  )
}

export default App
