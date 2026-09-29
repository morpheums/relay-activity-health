import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LocationHealth } from '../../../core/models';
import {
  beaconMixedHistoryReport,
  beaconNoEligibleWeeksReport,
  locationRow,
  withRange,
  withoutEnoughHistory,
} from '../../../../testing/activity-health-fixtures';
import { cellTexts, collapsedText, columnHeaderTexts, locationRows } from '../../../../testing/dom-queries';
import { LocationTable } from './location-table';

async function renderTable(locations: LocationHealth[], minimumEligibleWeeks = 4): Promise<HTMLElement> {
  TestBed.configureTestingModule({ imports: [LocationTable] });
  const fixture: ComponentFixture<LocationTable> = TestBed.createComponent(LocationTable);
  fixture.componentRef.setInput('locations', locations);
  fixture.componentRef.setInput('minimumEligibleWeeks', minimumEligibleWeeks);
  await fixture.whenStable();
  return fixture.nativeElement as HTMLElement;
}

function rowFor(root: HTMLElement, location: string): HTMLElement {
  const row = locationRows(root).find((candidate) => cellTexts(candidate).includes(location));
  if (!row) {
    throw new Error(`No row for ${location} in: ${collapsedText(root)}`);
  }
  return row;
}

function usualRangeCellText(root: HTMLElement, location: string): string {
  const usualRangeColumn = columnHeaderTexts(root).indexOf('Usual range');
  expect(usualRangeColumn).toBeGreaterThanOrEqual(0);
  return cellTexts(rowFor(root, location))[usualRangeColumn];
}

describe('LocationTable', () => {
  it('shows the column headers Location, Events, Usual range, Status', async () => {
    const root = await renderTable([locationRow('Site A', withRange(7, 7, 5, 17, 'normal', 0))]);

    expect(columnHeaderTexts(root)).toEqual(['Location', 'Events', 'Usual range', 'Status']);
  });

  it('renders rows in exactly the payload order C, A, D, B and prints low/high as given (UI-41)', async () => {
    const root = await renderTable([
      locationRow('Site C', withRange(7, 7.5, 7, 8, 'normal', 0.1)),
      locationRow('Site A', withRange(2, 7.5, 7, 8, 'below', -2.5)),
      locationRow('Site D', withRange(7, 7.5, 7, 8, 'normal', -1.0)),
      locationRow('Site B', withRange(12, 7.5, 7, 8, 'above', 3.0)),
    ]);

    const renderedLocations = locationRows(root).map((row) => cellTexts(row).find((cell) => /^Site [A-Z]$/.test(cell)));
    expect(renderedLocations).toEqual(['Site C', 'Site A', 'Site D', 'Site B']);
    locationRows(root).forEach((row) => expect(collapsedText(row)).toContain('Usually 7–8 a week'));
  });

  it('labels every status with symbol and text, taking weeks needed from minimumEligibleWeeks (UI-05)', async () => {
    const root = await renderTable(
      [
        locationRow('Site A', withRange(20, 6, 2, 12, 'above', 3.1)),
        locationRow('Site B', withRange(0, 7, 3, 13, 'below', -3.0)),
        locationRow('Site C', withRange(6, 6, 2, 12, 'normal', 0)),
        locationRow('Site D', withoutEnoughHistory(0, 2)),
      ],
      6,
    );

    expect(collapsedText(rowFor(root, 'Site A'))).toContain('▲ Higher than usual');
    expect(collapsedText(rowFor(root, 'Site B'))).toContain('▼ Lower than usual');
    expect(collapsedText(rowFor(root, 'Site C'))).toContain('Within usual range');
    expect(collapsedText(rowFor(root, 'Site D'))).toContain('Not enough history yet (2 of 6 weeks needed)');
  });

  it('leaves the "Usual range" cell empty for every insufficient row while showing its count and message (UI-45, account 14, 2026-02-02)', async () => {
    const root = await renderTable(beaconNoEligibleWeeksReport().locations);
    const expectedCounts: Record<string, string> = { 'Site A': '9', 'Site B': '5', 'Site C': '7', 'Site D': '6' };

    Object.entries(expectedCounts).forEach(([location, count]) => {
      expect(usualRangeCellText(root, location)).toBe('');
      expect(cellTexts(rowFor(root, location))).toContain(count);
      expect(collapsedText(rowFor(root, location))).toContain('Not enough history yet (0 of 4 weeks needed)');
    });
    expect(collapsedText(root)).not.toContain('Usually');
    expect(collapsedText(root)).not.toContain('0–0');
    locationRows(root).forEach((row) => expect(cellTexts(row).filter((cell) => /^[—–-]$/.test(cell))).toEqual([]));
  });

  it('empties the range only for insufficient rows in a mixed table (UI-45, account 14, 2026-03-02)', async () => {
    const root = await renderTable(beaconMixedHistoryReport().locations);

    expect(usualRangeCellText(root, 'Site A')).toBe('');
    expect(usualRangeCellText(root, 'Site C')).toBe('');
    expect(usualRangeCellText(root, 'Site D')).toBe('Usually 2–11 a week');
    expect(usualRangeCellText(root, 'Site B')).toBe('Usually 2–10 a week');
  });
});
