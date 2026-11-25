import { describe, expect, it } from "vitest";
import { parseDecimal, parseId } from "./form-data";

describe("parseDecimal", () => {
  it("accepts a comma or a point", () => {
    expect(parseDecimal("12,5")).toBe(12.5);
    expect(parseDecimal("12.5")).toBe(12.5);
    expect(parseDecimal(" 14 ")).toBe(14);
  });

  it("returns null for an empty field", () => {
    expect(parseDecimal("")).toBeNull();
    expect(parseDecimal("   ")).toBeNull();
  });

  it("returns NaN for text that is not a number", () => {
    expect(parseDecimal("abc")).toBeNaN();
    expect(parseDecimal("12,5,3")).toBeNaN();
    expect(parseDecimal("1e3")).toBeNaN();
  });

  it("keeps a negative number so that the API can refuse it with its own message", () => {
    expect(parseDecimal("-1")).toBe(-1);
  });
});

describe("parseId", () => {
  it("reads a select value", () => {
    expect(parseId("3")).toBe(3);
    expect(parseId("")).toBeNull();
    expect(parseId("abc")).toBeNull();
  });
});
