"use client";

import { init as initApm } from "@elastic/apm-rum";

let initialized = false;

export function initializeRum() {
  const serverUrl = process.env.NEXT_PUBLIC_ELASTIC_APM_SERVER_URL;

  if (initialized || !serverUrl) {
    return;
  }

  initialized = true;
  initApm({
    serviceName: "elkstack-flight-booking-web",
    serviceVersion: "0.1.0",
    serverUrl,
    serverUrlPrefix: "/rum",
    centralConfig: false,
  });
}
