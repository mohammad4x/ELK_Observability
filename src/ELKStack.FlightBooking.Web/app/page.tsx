import Link from "next/link";
import { getAvailableFlights } from "../lib/flights";

export const dynamic = "force-dynamic";

function time(value: string) {
  return new Intl.DateTimeFormat("en", { hour: "2-digit", minute: "2-digit", timeZone: "UTC" }).format(new Date(value));
}

export default async function FlightsPage() {
  const flights = await getAvailableFlights();

  return (
    <main>
      <header><p className="eyebrow">ELK observability demo</p><h1>Available flights</h1><p>Server-rendered flight inventory. Its request and catalog span are emitted through the OpenTelemetry SDK.</p></header>
      <section className="flight-list" aria-label="Available flights">
        {flights.map((flight) => <article className="flight-card" key={flight.id}>
          <div><strong>{flight.flightNumber}</strong><p>{flight.origin} → {flight.destination}</p></div>
          <div><span>{time(flight.departure)} – {time(flight.arrival)} UTC</span><strong>{flight.price.toLocaleString()} {flight.currency}</strong></div>
          <Link href={`/book?flight=${flight.id}`}>Choose flight</Link>
        </article>)}
      </section>
    </main>
  );
}
