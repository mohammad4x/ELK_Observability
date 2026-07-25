import { BookingForm } from "./booking-form";
import { Suspense } from "react";

export default function BookPage() {
  return <main><header><p className="eyebrow">Client-rendered page</p><h1>Passenger details</h1><p>This page is intentionally a client component so an Elastic RUM agent can be added in the next phase.</p></header><Suspense fallback={<p>Loading booking form…</p>}><BookingForm /></Suspense></main>;
}
