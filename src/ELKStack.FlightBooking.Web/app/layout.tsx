import type { Metadata } from "next";
import "./styles.css";

export const metadata: Metadata = {
  title: "Flight booking | ELK observability",
  description: "A small flight booking UI for the ELK observability demo."
};

export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return <html lang="en"><body>{children}</body></html>;
}
