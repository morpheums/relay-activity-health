import { addWeeks, isIsoMonday } from './week';

describe('week', () => {
  it.each<{ candidate: string | null; expected: boolean }>([
    { candidate: '2026-07-20', expected: true },
    { candidate: '2026-07-21', expected: false },
    { candidate: '2026-02-30', expected: false },
    { candidate: '2026-7-20', expected: false },
    { candidate: '', expected: false },
    { candidate: null, expected: false },
  ])('isIsoMonday($candidate) is $expected', ({ candidate, expected }) => {
    expect(isIsoMonday(candidate)).toBe(expected);
  });

  it.each([
    { crossing: 'no boundary (previous)', weekStart: '2026-07-20', weekCount: -1, expectedWeek: '2026-07-13' },
    { crossing: 'no boundary (next)', weekStart: '2026-07-13', weekCount: 1, expectedWeek: '2026-07-20' },
    { crossing: 'a month boundary', weekStart: '2026-03-02', weekCount: -1, expectedWeek: '2026-02-23' },
    { crossing: 'a year boundary', weekStart: '2025-12-29', weekCount: 1, expectedWeek: '2026-01-05' },
    { crossing: 'US DST start 2026-03-08', weekStart: '2026-03-09', weekCount: -1, expectedWeek: '2026-03-02' },
    { crossing: 'US DST end 2026-11-01', weekStart: '2026-10-26', weekCount: 1, expectedWeek: '2026-11-02' },
  ])('addWeeks steps Monday to Monday across $crossing', ({ weekStart, weekCount, expectedWeek }) => {
    expect(addWeeks(weekStart, weekCount)).toBe(expectedWeek);
  });
});
