import { NextResponse } from "next/server";

export async function POST(request: Request) {
  const serviceUrl = process.env.BOOKING_SERVICE_URL;
  if (!serviceUrl) return NextResponse.json({ message: "Booking service is not configured." }, { status: 503 });

  const response = await fetch(new URL("/api/bookings", serviceUrl), { method: "POST", headers: { "content-type": "application/json" }, body: await request.text(), cache: "no-store" });
  const body = await response.text();
  return new NextResponse(body, { status: response.status, headers: { "content-type": response.headers.get("content-type") ?? "application/json" } });
}
