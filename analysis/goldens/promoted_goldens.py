import os
GOLDEN = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'statistician', 'golden.py')
source = open(GOLDEN).read().split("show(6,'2026-06-01')")[0]
__file__ = os.path.abspath(GOLDEN)
exec(source)
for scenario in [(6, '2026-06-08'), (6, '2026-06-29'), (8, '2026-07-20'), (8, '2026-03-02'), (14, '2026-01-26'), (14, '2026-02-02'),
                 (14, '2026-03-02'), (14, '2026-07-20'), (14, '2026-07-20', 'appointment_set'), (18, '2026-03-23')]:
    show(*scenario)
