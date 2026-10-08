export interface TimesheetEntry {
  date: string
  hoursWorked: number
  isPublicHoliday: boolean
}

export interface PayLine {
  ruleName: string
  date: string
  hours: number
  rate: number
  pay: number
}

export interface PayBreakdown {
  id: number | null
  employeeName: string
  baseHourlyRate: number
  totalHours: number
  totalPay: number
  ordinaryHours: number
  ordinaryPay: number
  overtimeHours: number
  overtimePay: number
  publicHolidayHours: number
  publicHolidayPay: number
  lines: PayLine[]
}

export interface PayRunSummary {
  id: number
  employeeName: string
  totalHours: number
  totalPay: number
  createdAtUtc: string
}

async function request<T>(method: 'GET' | 'POST', url: string, body?: unknown): Promise<T> {
  const response = await fetch(url, {
    method,
    headers: body ? { 'Content-Type': 'application/json' } : undefined,
    body: body ? JSON.stringify(body) : undefined,
  })

  if (!response.ok) {
    const data = await response.json().catch(() => null)
    throw new Error(data?.error ?? `Request failed (${response.status})`)
  }

  return response.json()
}

export const api = {
  calculate: (employeeName: string, baseHourlyRate: number, entries: TimesheetEntry[]) =>
    request<PayBreakdown>('POST', '/api/payruns', { employeeName, baseHourlyRate, entries }),

  listRuns: () => request<PayRunSummary[]>('GET', '/api/payruns'),

  getRun: (id: number) => request<PayBreakdown>('GET', `/api/payruns/${id}`),
}
