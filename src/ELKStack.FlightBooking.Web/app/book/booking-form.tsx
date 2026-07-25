"use client";

import { FormEvent, useEffect, useState } from "react";
import { useSearchParams } from "next/navigation";
import { initializeRum } from "../../lib/rum";

type Result = { bookingId: string; status: string };

export function BookingForm() {
  const params = useSearchParams();
  const [result, setResult] = useState<Result | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    initializeRum();
  }, []);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSubmitting(true); setError(null); setResult(null);
    const form = new FormData(event.currentTarget);
    const response = await fetch("/api/bookings", { method: "POST", headers: { "content-type": "application/json" }, body: JSON.stringify({
      passengerName: form.get("passengerName"), customerEmail: form.get("customerEmail"), destination: form.get("destination"), amount: Number(form.get("amount")), currency: form.get("currency")
    }) });
    const body = await response.json().catch(() => null);
    setSubmitting(false);
    if (!response.ok) { setError(body?.message ?? "Booking could not be submitted."); return; }
    setResult(body);
  }

  return <form className="booking-form" onSubmit={submit}>
    <label>Passenger name<input name="passengerName" required placeholder="Sara Ahmadi" /></label>
    <label>Email<input name="customerEmail" type="email" required placeholder="sara@example.com" /></label>
    <label>Destination<input name="destination" required defaultValue={params.get("flight") ? "Berlin" : ""} placeholder="Berlin" /></label>
    <div className="form-row"><label>Amount<input name="amount" type="number" min="1" required defaultValue="1490" /></label><label>Currency<select name="currency" defaultValue="EUR"><option>EUR</option><option>USD</option></select></label></div>
    <button disabled={submitting}>{submitting ? "Submitting…" : "Book flight"}</button>
    {result && <p className="success">Booking {result.bookingId} accepted with status: {result.status}.</p>}
    {error && <p className="error">{error}</p>}
  </form>;
}
