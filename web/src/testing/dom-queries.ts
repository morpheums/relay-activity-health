export function collapsedText(element: Element | null | undefined): string {
  return (element?.textContent ?? '').replace(/\s+/g, ' ').trim();
}

function accessibleNameOf(element: Element): string {
  return element.getAttribute('aria-label')?.trim() ?? collapsedText(element);
}

export function findButton(root: Element, name: string): HTMLButtonElement | null {
  const buttons = Array.from(root.querySelectorAll('button'));
  return buttons.find((button) => accessibleNameOf(button) === name) ?? null;
}

export function getButton(root: Element, name: string): HTMLButtonElement {
  const button = findButton(root, name);
  if (!button) {
    throw new Error(`No button named "${name}" in: ${collapsedText(root)}`);
  }
  return button;
}

export function isDisabled(button: HTMLButtonElement): boolean {
  return button.disabled || button.getAttribute('aria-disabled') === 'true';
}

function ownLabelText(label: Element): string {
  const withoutControls = label.cloneNode(true) as Element;
  withoutControls.querySelectorAll('select, option, input').forEach((control) => control.remove());
  return collapsedText(withoutControls);
}

export function findSelect(root: Element, labelText: string): HTMLSelectElement | null {
  const byAriaLabel = Array.from(root.querySelectorAll('select')).find((select) => select.getAttribute('aria-label')?.trim() === labelText);
  if (byAriaLabel) {
    return byAriaLabel;
  }
  const labelElements = Array.from(root.querySelectorAll('label, [id]')).filter((candidate) => ownLabelText(candidate) === labelText);
  for (const labelElement of labelElements) {
    const nested = labelElement.querySelector('select');
    if (nested) {
      return nested;
    }
    const targetId = labelElement.getAttribute('for');
    const byFor = targetId ? root.querySelector(`select#${CSS.escape(targetId)}`) : null;
    if (byFor instanceof HTMLSelectElement) {
      return byFor;
    }
    const labelId = labelElement.getAttribute('id');
    const byLabelledBy = labelId
      ? Array.from(root.querySelectorAll('select')).find((select) => (select.getAttribute('aria-labelledby') ?? '').split(/\s+/).includes(labelId))
      : undefined;
    if (byLabelledBy) {
      return byLabelledBy;
    }
  }
  return null;
}

export function getSelect(root: Element, labelText: string): HTMLSelectElement {
  const select = findSelect(root, labelText);
  if (!select) {
    throw new Error(`No select labelled "${labelText}" in: ${collapsedText(root)}`);
  }
  return select;
}

export function optionTexts(select: HTMLSelectElement): string[] {
  return Array.from(select.options).map((option) => collapsedText(option));
}

export function selectedOptionText(select: HTMLSelectElement): string {
  return collapsedText(select.selectedOptions.item(0));
}

export function chooseOption(select: HTMLSelectElement, optionText: string): void {
  const option = Array.from(select.options).find((candidate) => collapsedText(candidate) === optionText);
  if (!option) {
    throw new Error(`No option "${optionText}" in select with options: ${optionTexts(select).join(', ')}`);
  }
  select.value = option.value;
  select.dispatchEvent(new Event('input', { bubbles: true }));
  select.dispatchEvent(new Event('change', { bubbles: true }));
}

export function locationRows(root: Element): HTMLElement[] {
  const tableRows = Array.from(root.querySelectorAll<HTMLElement>('tr')).filter((row) => row.querySelector('td') !== null);
  const ariaRows = Array.from(root.querySelectorAll<HTMLElement>('[role="row"]')).filter(
    (row) => row.tagName !== 'TR' && row.querySelector('[role="cell"], [role="gridcell"]') !== null,
  );
  return [...tableRows, ...ariaRows];
}

export function cellTexts(row: Element): string[] {
  return Array.from(row.querySelectorAll('td, th, [role="cell"], [role="gridcell"], [role="rowheader"]')).map((cell) => collapsedText(cell));
}

export function columnHeaderTexts(root: Element): string[] {
  return Array.from(root.querySelectorAll('th, [role="columnheader"]'))
    .filter((header) => header.closest('tbody') === null && header.getAttribute('scope') !== 'row' && header.getAttribute('role') !== 'rowheader')
    .map((header) => collapsedText(header));
}

export function hasTable(root: Element): boolean {
  return root.querySelector('table, [role="table"], [role="grid"]') !== null;
}

export function textOutsideTables(root: Element): string {
  const withoutTables = root.cloneNode(true) as Element;
  withoutTables.querySelectorAll('table, [role="table"], [role="grid"]').forEach((table) => table.remove());
  return collapsedText(withoutTables);
}
