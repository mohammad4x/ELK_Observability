import { trace } from "@opentelemetry/api";

export type Flight = {
  id: string;
  flightNumber: string;
  origin: string;
  destination: string;
  departure: string;
  arrival: string;
  price: number;
  currency: string;
};

const flights: Flight[] = [
  { id: "fl-101", flightNumber: "EL 101", origin: "Tehran", destination: "Berlin", departure: "2026-08-12T06:20:00Z", arrival: "2026-08-12T10:45:00Z", price: 1490, currency: "EUR" },
  { id: "fl-202", flightNumber: "EL 202", origin: "Tehran", destination: "Istanbul", departure: "2026-08-12T09:00:00Z", arrival: "2026-08-12T12:15:00Z", price: 760, currency: "EUR" },
  { id: "fl-303", flightNumber: "EL 303", origin: "Tehran", destination: "Dubai", departure: "2026-08-12T14:40:00Z", arrival: "2026-08-12T16:50:00Z", price: 890, currency: "USD" },
  { id: "fl-404", flightNumber: "EL 404", origin: "Tehran", destination: "Paris", departure: "2026-08-13T07:30:00Z", arrival: "2026-08-13T13:05:00Z", price: 1720, currency: "EUR" }
];

export async function getAvailableFlights(): Promise<Flight[]> {
  return trace.getTracer("flight-catalog").startActiveSpan("flight.catalog.list", (span) => {
    span.setAttribute("flight.catalog.count", flights.length);
    span.end();
    return flights;
  });
}

export function getFlight(id: string) {
  return flights.find((flight) => flight.id === id);
}
