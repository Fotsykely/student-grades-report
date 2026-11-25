import { describe, expect, it } from "vitest";
import { formatAverage, formatGrade, formatGradeCount, formatRank } from "./format";

describe("format", () => {
  it("shows a grade with at most 2 decimals and a decimal comma", () => {
    expect(formatGrade(12.5)).toBe("12,5");
    expect(formatGrade(14)).toBe("14");
    expect(formatGrade(9.25)).toBe("9,25");
  });

  it("always shows 2 decimals for an average", () => {
    expect(formatAverage(12)).toBe("12,00");
    expect(formatAverage(12.11)).toBe("12,11");
  });

  it("writes ranks, with ties", () => {
    expect(formatRank(1, false)).toBe("1er");
    expect(formatRank(2, false)).toBe("2e");
    expect(formatRank(2, true)).toBe("2e ex æquo");
  });

  it("pluralises the grade count", () => {
    expect(formatGradeCount(1)).toBe("1 note");
    expect(formatGradeCount(5)).toBe("5 notes");
  });
});
