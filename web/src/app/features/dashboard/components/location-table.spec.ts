import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LocationHealth } from '../../../core/models';
import { locationRow, withRange, withoutEnoughHistory } from '../../../../testing/activity-health-fixtures';
import { cellTexts, collapsedText, columnHeaderTexts, locationRows } from '../../../../testing/dom-queries';
import { LocationTable } from './location-table';

const FORBIDDEN_ON_SCREEN: RegExp[] = [/\bz\b/, /σ/, /±/, /\bmedian\b/i, /\btypical\b/i, /\bdeviation\b/i, /\bNormal\b/];

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

function unsortedPayloadWithUnreproducibleRanges(): LocationHealth[] {
  return [
    locationRow('Site C', withRange(7, 7.5, 7, 8, 'normal', 0.1)),
    locationRow('Site A', withRange(2, 7.5, 7, 8, 'below', -2.5)),
    locationRow('Site D', withRange(7, 7.5, 7, 8, 'normal', -1.0)),
    locationRow('Site B', withRange(12, 7.5, 7, 8, 'above', 3.0)),
  ];
}

describe('LocationTable', () => {
  it('shows the column headers Location, Events, Usual range, Status', async () => {
    const root = await renderTable(unsortedPayloadWithUnreproducibleRanges());

    expect(columnHeaderTexts(root)).toEqual(['Location', 'Events', 'Usual range', 'Status']);
  });

  it('renders the rows in exactly the payload order C, A, D, B without re-sorting', async () => {
    const root = await renderTable(unsortedPayloadWithUnreproducibleRanges());

    const renderedLocations = locationRows(root).map((row) => cellTexts(row).find((cell) => /^Site [A-Z]$/.test(cell)));
    expect(renderedLocations).toEqual(['Site C', 'Site A', 'Site D', 'Site B']);
  });

  it('prints low and high exactly as given ("Usually 7–8 a week") without recomputing them', async () => {
    const root = await renderTable(unsortedPayloadWithUnreproducibleRanges());

    const rows = locationRows(root);
    expect(rows).toHaveLength(4);
    rows.forEach((row) => expect(collapsedText(row)).toContain('Usually 7–8 a week'));
  });

  it('shows each count, including 0', async () => {
    const root = await renderTable([
      locationRow('Site G', withRange(0, 4.5, 2, 9, 'below', -3.19)),
      locationRow('Site N', withRange(2, 4.5, 2, 9, 'normal', -1.33)),
    ]);

    expect(cellTexts(rowFor(root, 'Site G'))).toContain('0');
    expect(cellTexts(rowFor(root, 'Site N'))).toContain('2');
  });

  it.each([
    { status: 'above', row: locationRow('Site B', withRange(12, 7.5, 7, 8, 'above', 3.0)), label: '▲ Higher than usual' },
    { status: 'below', row: locationRow('Site B', withRange(2, 6.5, 3, 12, 'below', -2.16)), label: '▼ Lower than usual' },
    { status: 'normal', row: locationRow('Site B', withRange(9, 6, 2, 12, 'normal', 0.98)), label: 'Within usual range' },
  ])('labels status $status as "$label" (symbol and text together)', async ({ row, label }) => {
    const root = await renderTable([row]);

    expect(collapsedText(rowFor(root, 'Site B'))).toContain(label);
  });

  it('shows "Not enough history yet (3 of 4 weeks needed)" with the count and no range for an insufficient row', async () => {
    const root = await renderTable([locationRow('Site A', withoutEnoughHistory(7, 3))]);

    const row = rowFor(root, 'Site A');
    expect(cellTexts(row)).toContain('7');
    expect(collapsedText(row)).toContain('Not enough history yet (3 of 4 weeks needed)');
    expect(collapsedText(row)).not.toContain('Usually');
  });

  it('keeps the 0 case: "Not enough history yet (0 of 4 weeks needed)"', async () => {
    const root = await renderTable([locationRow('Site B', withoutEnoughHistory(1, 0))]);

    expect(collapsedText(rowFor(root, 'Site B'))).toContain('Not enough history yet (0 of 4 weeks needed)');
  });

  it('takes N from baseline.weeksUsed and the weeks needed from the minimumEligibleWeeks input', async () => {
    const root = await renderTable([locationRow('Site A', withoutEnoughHistory(9, 3))], 6);

    expect(collapsedText(rowFor(root, 'Site A'))).toContain('Not enough history yet (3 of 6 weeks needed)');
  });

  it('never shows deviation, median, z, σ, ± or a standalone "Normal"', async () => {
    const root = await renderTable(unsortedPayloadWithUnreproducibleRanges());

    expect(collapsedText(root)).toContain('Site C');
    FORBIDDEN_ON_SCREEN.forEach((forbidden) => expect(collapsedText(root)).not.toMatch(forbidden));
    expect(collapsedText(root)).not.toContain('7.5');
    expect(collapsedText(root)).not.toContain('2.5');
  });
});
