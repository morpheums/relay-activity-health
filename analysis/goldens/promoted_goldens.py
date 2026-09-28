import os
GOLDEN = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'statistician', 'golden.py')
source = open(GOLDEN).read().split("show(6,'2026-06-01')")[0]
__file__ = os.path.abspath(GOLDEN)
exec(source)
for scenario in [(6, '2026-06-08'), (6, '2026-06-29'), (8, '2026-07-20'), (8, '2026-03-02'), (14, '2026-01-26'), (14, '2026-02-02'),
                 (14, '2026-03-02'), (14, '2026-07-20'), (14, '2026-07-20', 'appointment_set'), (18, '2026-03-23')]:
    show(*scenario)
show(6, '2026-07-20', top=1)
show(14, '2026-07-20', 'call_received', sites=False)
show(14, '2026-07-20', 'lead_created', sites=False)
print('\n### de-duplicated events per account (all weeks)')
per_account = {account: sum(n for (a, series, event_type, week), n in cnt.items() if a == account and series == '*' and event_type == 'all') for account in range(1, 21)}
for account, total in per_account.items():
    print(f'| {account} | {total} |')
print(f'| total | {sum(per_account.values())} |')
print('\n### account 14 site first-activity weeks')
for (a, series), first in sorted(firstev.items()):
    if a == 14 and series != '*':
        print(f'| {series} | {first.isoformat()} | week {week_of(first, tzs[a])} |')
