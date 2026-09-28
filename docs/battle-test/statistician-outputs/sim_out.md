## A. Main table, default thresholds, full 8-week baselines, spike week excluded

### site / all: 1105 weeks
| rule | false above | false below | drop→0 (med≥3) | −50% (med≥6) det / stoch | +100% (med≥3) det / stoch | low edge 0 | contradictions |
|---|---|---|---|---|---|---|---|
| R0 current | 3.8% | 1.0% |  77.8% (n=1104) |   0.0% (n=770) /  15.8% (n=770) |  77.8% (n=1104) /  57.8% (n=1104) | 22.3% | 0 |
| R1 current+zero | 3.8% | 1.1% | 100.0% (n=1104) |   0.0% (n=770) /  18.6% (n=770) |  77.8% (n=1104) /  57.8% (n=1104) | 22.3% | 1 |
| R2 anscombe | 1.4% | 2.9% |  97.6% (n=1104) |   5.7% (n=770) /  31.7% (n=770) |  48.4% (n=1104) /  46.9% (n=1104) | 2.4% | 0 |
| R3 freeman-tukey | 1.4% | 2.9% |  99.5% (n=1104) |   5.7% (n=770) /  30.9% (n=770) |  47.6% (n=1104) /  46.8% (n=1104) | 0.5% | 0 |
| R4 poisson/NB exact | 1.7% | 1.2% |  88.8% (n=1104) |   0.0% (n=770) /  19.4% (n=770) |  56.5% (n=1104) /  50.5% (n=1104) | 11.3% | 0 |

### site / call_received: 1105 weeks
| rule | false above | false below | drop→0 (med≥3) | −50% (med≥6) det / stoch | +100% (med≥3) det / stoch | low edge 0 | contradictions |
|---|---|---|---|---|---|---|---|
| R0 current | 3.2% | 0.8% |  36.5% (n=967) |   0.0% (n=129) /  25.6% (n=129) |  36.5% (n=967) /  43.3% (n=967) | 68.1% | 0 |
| R1 current+zero | 3.2% | 2.4% | 100.0% (n=967) |   0.0% (n=129) /  19.4% (n=129) |  36.5% (n=967) /  43.3% (n=967) | 68.1% | 17 |
| R2 anscombe | 1.0% | 3.8% |  95.3% (n=967) |   0.0% (n=129) /  31.0% (n=129) |   4.4% (n=967) /  35.4% (n=967) | 13.7% | 0 |
| R3 freeman-tukey | 1.0% | 4.3% |  97.4% (n=967) |   0.0% (n=129) /  34.1% (n=129) |   3.6% (n=967) /  35.4% (n=967) | 8.0% | 0 |
| R4 poisson/NB exact | 1.5% | 1.0% |  55.8% (n=967) |   0.0% (n=129) /  24.8% (n=129) |   9.7% (n=967) /  37.2% (n=967) | 51.1% | 0 |

### site / lead_created: 1105 weeks
| rule | false above | false below | drop→0 (med≥3) | −50% (med≥6) det / stoch | +100% (med≥3) det / stoch | low edge 0 | contradictions |
|---|---|---|---|---|---|---|---|
| R0 current | 5.9% | 0.0% |   0.0% (n=48) |    n/a (n=0) /    n/a (n=0) |   0.0% (n=48) /   8.3% (n=48) | 100.0% | 0 |
| R1 current+zero | 5.9% | 0.7% | 100.0% (n=48) |    n/a (n=0) /    n/a (n=0) |   0.0% (n=48) /   8.3% (n=48) | 100.0% | 8 |
| R2 anscombe | 1.4% | 1.7% |  95.8% (n=48) |    n/a (n=0) /    n/a (n=0) |   0.0% (n=48) /   8.3% (n=48) | 90.1% | 0 |
| R3 freeman-tukey | 2.1% | 5.2% | 100.0% (n=48) |    n/a (n=0) /    n/a (n=0) |   0.0% (n=48) /   8.3% (n=48) | 65.7% | 0 |
| R4 poisson/NB exact | 3.2% | 0.0% |   6.2% (n=48) |    n/a (n=0) /    n/a (n=0) |   0.0% (n=48) /   8.3% (n=48) | 99.7% | 0 |

### site / appointment_set: 1105 weeks
| rule | false above | false below | drop→0 (med≥3) | −50% (med≥6) det / stoch | +100% (med≥3) det / stoch | low edge 0 | contradictions |
|---|---|---|---|---|---|---|---|
| R0 current | 4.3% | 0.0% |    n/a (n=0) |    n/a (n=0) /    n/a (n=0) |    n/a (n=0) /    n/a (n=0) | 100.0% | 0 |
| R1 current+zero | 4.3% | 0.0% |    n/a (n=0) |    n/a (n=0) /    n/a (n=0) |    n/a (n=0) /    n/a (n=0) | 100.0% | 0 |
| R2 anscombe | 1.7% | 0.0% |    n/a (n=0) |    n/a (n=0) /    n/a (n=0) |    n/a (n=0) /    n/a (n=0) | 99.9% | 0 |
| R3 freeman-tukey | 4.9% | 1.3% |    n/a (n=0) |    n/a (n=0) /    n/a (n=0) |    n/a (n=0) /    n/a (n=0) | 95.0% | 0 |
| R4 poisson/NB exact | 4.1% | 0.0% |    n/a (n=0) |    n/a (n=0) /    n/a (n=0) |    n/a (n=0) /    n/a (n=0) | 100.0% | 0 |

### account / all: 311 weeks
| rule | false above | false below | drop→0 (med≥3) | −50% (med≥6) det / stoch | +100% (med≥3) det / stoch | low edge 0 | contradictions |
|---|---|---|---|---|---|---|---|
| R0 current | 2.6% | 2.6% |  99.4% (n=311) |  52.1% (n=311) /  53.1% (n=311) |  99.4% (n=311) /  84.6% (n=311) | 0.6% | 0 |
| R1 current+zero | 2.6% | 2.6% | 100.0% (n=311) |  52.1% (n=311) /  52.7% (n=311) |  99.4% (n=311) /  84.6% (n=311) | 0.6% | 0 |
| R2 anscombe | 1.6% | 3.2% | 100.0% (n=311) |  72.7% (n=311) /  59.2% (n=311) |  96.8% (n=311) /  79.7% (n=311) | 0.0% | 0 |
| R3 freeman-tukey | 1.6% | 3.2% | 100.0% (n=311) |  72.7% (n=311) /  63.3% (n=311) |  96.8% (n=311) /  79.7% (n=311) | 0.0% | 0 |
| R4 poisson/NB exact | 1.6% | 2.6% | 100.0% (n=311) |  57.6% (n=311) /  53.1% (n=311) |  97.7% (n=311) /  81.0% (n=311) | 0.0% | 0 |

### account / call_received: 311 weeks
| rule | false above | false below | drop→0 (med≥3) | −50% (med≥6) det / stoch | +100% (med≥3) det / stoch | low edge 0 | contradictions |
|---|---|---|---|---|---|---|---|
| R0 current | 2.9% | 1.9% |  93.2% (n=309) |  32.1% (n=271) /  42.4% (n=271) |  93.2% (n=309) /  70.9% (n=309) | 7.4% | 0 |
| R1 current+zero | 2.9% | 1.9% | 100.0% (n=309) |  32.1% (n=271) /  39.9% (n=271) |  93.2% (n=309) /  70.9% (n=309) | 7.4% | 0 |
| R2 anscombe | 1.6% | 4.5% |  98.4% (n=309) |  50.9% (n=271) /  55.7% (n=271) |  77.7% (n=309) /  65.7% (n=309) | 2.3% | 0 |
| R3 freeman-tukey | 1.6% | 4.5% |  99.7% (n=309) |  50.9% (n=271) /  48.3% (n=271) |  76.7% (n=309) /  65.7% (n=309) | 1.0% | 0 |
| R4 poisson/NB exact | 1.6% | 2.6% |  98.1% (n=309) |  36.9% (n=271) /  43.2% (n=271) |  84.5% (n=309) /  67.6% (n=309) | 2.6% | 0 |

### account / lead_created: 311 weeks
| rule | false above | false below | drop→0 (med≥3) | −50% (med≥6) det / stoch | +100% (med≥3) det / stoch | low edge 0 | contradictions |
|---|---|---|---|---|---|---|---|
| R0 current | 4.2% | 0.6% |  58.8% (n=257) |   4.2% (n=119) /  14.3% (n=119) |  58.8% (n=257) /  55.6% (n=257) | 51.4% | 0 |
| R1 current+zero | 4.2% | 1.6% | 100.0% (n=257) |   4.2% (n=119) /  18.5% (n=119) |  58.8% (n=257) /  55.6% (n=257) | 51.4% | 3 |
| R2 anscombe | 0.6% | 2.9% |  92.6% (n=257) |  20.2% (n=119) /  26.1% (n=119) |  27.2% (n=257) /  46.3% (n=257) | 19.9% | 0 |
| R3 freeman-tukey | 0.6% | 3.5% |  96.9% (n=257) |  20.2% (n=119) /  26.9% (n=119) |  27.2% (n=257) /  46.3% (n=257) | 10.0% | 0 |
| R4 poisson/NB exact | 1.6% | 0.6% |  68.5% (n=257) |  11.8% (n=119) /  13.4% (n=119) |  36.6% (n=257) /  48.6% (n=257) | 43.4% | 0 |

### account / appointment_set: 311 weeks
| rule | false above | false below | drop→0 (med≥3) | −50% (med≥6) det / stoch | +100% (med≥3) det / stoch | low edge 0 | contradictions |
|---|---|---|---|---|---|---|---|
| R0 current | 4.5% | 0.0% |  48.3% (n=149) |   0.0% (n=40) /  17.5% (n=40) |  48.3% (n=149) /  44.3% (n=149) | 76.8% | 0 |
| R1 current+zero | 4.5% | 1.0% | 100.0% (n=149) |   0.0% (n=40) /  12.5% (n=40) |  48.3% (n=149) /  44.3% (n=149) | 76.8% | 3 |
| R2 anscombe | 0.6% | 1.6% |  85.2% (n=149) |  17.5% (n=40) /  30.0% (n=40) |  20.1% (n=149) /  38.9% (n=149) | 54.0% | 0 |
| R3 freeman-tukey | 1.3% | 4.2% |  89.9% (n=149) |  17.5% (n=40) /  35.0% (n=40) |  20.1% (n=149) /  38.9% (n=149) | 38.3% | 0 |
| R4 poisson/NB exact | 1.6% | 0.0% |  60.4% (n=149) |   0.0% (n=40) /  25.0% (n=40) |  22.8% (n=149) /  40.9% (n=149) | 71.1% | 0 |

## B. Short baselines: real full baselines truncated to most recent 4/5/6 weeks (plus genuine 4–7-week rows)

### site / all / baseline=4: 1105 weeks
| rule | false above | false below | drop→0 | −50% det | +100% det | low edge 0 | contradictions |
|---|---|---|---|---|---|---|---|
| R0 current | 5.3% | 1.7% |  80.1% (n=1095) |   0.0% (n=752) |  80.1% (n=1095) | 20.6% | 0 |
| R2 anscombe | 2.4% | 3.5% |  97.1% (n=1095) |   9.6% (n=752) |  52.7% (n=1095) | 3.1% | 0 |
| R3 freeman-tukey | 2.5% | 3.5% |  98.0% (n=1095) |   9.6% (n=752) |  50.1% (n=1095) | 2.2% | 0 |
| R4 poisson/NB exact | 3.1% | 1.7% |  89.6% (n=1095) |   0.0% (n=752) |  58.5% (n=1095) | 11.2% | 0 |

### site / all / baseline=5: 1105 weeks
| rule | false above | false below | drop→0 | −50% det | +100% det | low edge 0 | contradictions |
|---|---|---|---|---|---|---|---|
| R0 current | 5.3% | 1.4% |  72.4% (n=1101) |   0.0% (n=781) |  72.4% (n=1101) | 27.9% | 0 |
| R2 anscombe | 1.9% | 3.5% |  94.5% (n=1101) |   4.2% (n=781) |  44.6% (n=1101) | 5.9% | 0 |
| R3 freeman-tukey | 1.9% | 3.5% |  97.7% (n=1101) |   4.2% (n=781) |  44.9% (n=1101) | 2.6% | 0 |
| R4 poisson/NB exact | 2.7% | 1.8% |  86.7% (n=1101) |   0.0% (n=781) |  57.5% (n=1101) | 13.6% | 0 |

### site / all / baseline=6: 1105 weeks
| rule | false above | false below | drop→0 | −50% det | +100% det | low edge 0 | contradictions |
|---|---|---|---|---|---|---|---|
| R0 current | 3.6% | 1.5% |  77.6% (n=1101) |   0.0% (n=757) |  77.6% (n=1101) | 22.7% | 0 |
| R2 anscombe | 1.7% | 3.3% |  97.6% (n=1101) |   6.3% (n=757) |  50.4% (n=1101) | 2.4% | 0 |
| R3 freeman-tukey | 1.7% | 3.4% |  99.1% (n=1101) |   6.3% (n=757) |  48.8% (n=1101) | 0.9% | 0 |
| R4 poisson/NB exact | 2.1% | 1.7% |  89.6% (n=1101) |   0.0% (n=757) |  56.9% (n=1101) | 10.8% | 0 |

### site / all / baseline=8: 1105 weeks
| rule | false above | false below | drop→0 | −50% det | +100% det | low edge 0 | contradictions |
|---|---|---|---|---|---|---|---|
| R0 current | 3.8% | 1.0% |  77.8% (n=1104) |   0.0% (n=770) |  77.8% (n=1104) | 22.3% | 0 |
| R2 anscombe | 1.4% | 2.9% |  97.6% (n=1104) |   5.7% (n=770) |  48.4% (n=1104) | 2.4% | 0 |
| R3 freeman-tukey | 1.4% | 2.9% |  99.5% (n=1104) |   5.7% (n=770) |  47.6% (n=1104) | 0.5% | 0 |
| R4 poisson/NB exact | 1.7% | 1.2% |  88.8% (n=1104) |   0.0% (n=770) |  56.5% (n=1104) | 11.3% | 0 |

### site / all / baseline=genuine 4-7: 276 weeks
| rule | false above | false below | drop→0 | −50% det | +100% det | low edge 0 | contradictions |
|---|---|---|---|---|---|---|---|
| R0 current | 6.5% | 2.2% |  69.5% (n=272) |   0.0% (n=178) |  69.5% (n=272) | 31.5% | 0 |
| R2 anscombe | 2.2% | 3.6% |  96.0% (n=272) |   6.7% (n=178) |  37.9% (n=272) | 5.1% | 0 |
| R3 freeman-tukey | 2.2% | 3.6% |  97.8% (n=272) |   6.7% (n=178) |  37.9% (n=272) | 2.2% | 0 |
| R4 poisson/NB exact | 3.3% | 2.2% |  86.4% (n=272) |   0.0% (n=178) |  47.1% (n=272) | 14.9% | 0 |

### site / call_received / baseline=4: 1105 weeks
| rule | false above | false below | drop→0 | −50% det | +100% det | low edge 0 | contradictions |
|---|---|---|---|---|---|---|---|
| R0 current | 5.9% | 1.1% |  43.8% (n=927) |   0.0% (n=162) |  43.8% (n=927) | 63.3% | 0 |
| R2 anscombe | 2.3% | 4.2% |  92.1% (n=927) |   1.2% (n=162) |  11.3% (n=927) | 15.3% | 0 |
| R3 freeman-tukey | 2.3% | 4.6% |  94.5% (n=927) |   1.2% (n=162) |   8.6% (n=927) | 9.0% | 0 |
| R4 poisson/NB exact | 2.9% | 1.4% |  59.7% (n=927) |   0.0% (n=162) |  14.6% (n=927) | 50.0% | 0 |

### site / call_received / baseline=5: 1105 weeks
| rule | false above | false below | drop→0 | −50% det | +100% det | low edge 0 | contradictions |
|---|---|---|---|---|---|---|---|
| R0 current | 6.2% | 1.4% |  32.4% (n=971) |   0.0% (n=198) |  32.4% (n=971) | 71.5% | 0 |
| R2 anscombe | 2.0% | 3.8% |  89.7% (n=971) |   0.0% (n=198) |   6.5% (n=971) | 21.2% | 0 |
| R3 freeman-tukey | 2.0% | 4.3% |  93.7% (n=971) |   0.0% (n=198) |   6.5% (n=971) | 13.6% | 0 |
| R4 poisson/NB exact | 3.1% | 1.7% |  63.7% (n=971) |   0.0% (n=198) |  15.7% (n=971) | 44.0% | 0 |

### site / call_received / baseline=6: 1105 weeks
| rule | false above | false below | drop→0 | −50% det | +100% det | low edge 0 | contradictions |
|---|---|---|---|---|---|---|---|
| R0 current | 4.3% | 1.0% |  40.1% (n=956) |   0.0% (n=142) |  40.1% (n=956) | 65.3% | 0 |
| R2 anscombe | 1.4% | 4.1% |  93.8% (n=956) |   0.0% (n=142) |   6.8% (n=956) | 13.8% | 0 |
| R3 freeman-tukey | 1.4% | 4.4% |  95.2% (n=956) |   0.0% (n=142) |   5.4% (n=956) | 9.0% | 0 |
| R4 poisson/NB exact | 1.7% | 1.1% |  57.4% (n=956) |   0.0% (n=142) |  11.1% (n=956) | 50.3% | 0 |

### site / call_received / baseline=8: 1105 weeks
| rule | false above | false below | drop→0 | −50% det | +100% det | low edge 0 | contradictions |
|---|---|---|---|---|---|---|---|
| R0 current | 3.2% | 0.8% |  36.5% (n=967) |   0.0% (n=129) |  36.5% (n=967) | 68.1% | 0 |
| R2 anscombe | 1.0% | 3.8% |  95.3% (n=967) |   0.0% (n=129) |   4.4% (n=967) | 13.7% | 0 |
| R3 freeman-tukey | 1.0% | 4.3% |  97.4% (n=967) |   0.0% (n=129) |   3.6% (n=967) | 8.0% | 0 |
| R4 poisson/NB exact | 1.5% | 1.0% |  55.8% (n=967) |   0.0% (n=129) |   9.7% (n=967) | 51.1% | 0 |

### site / call_received / baseline=genuine 4-7: 276 weeks
| rule | false above | false below | drop→0 | −50% det | +100% det | low edge 0 | contradictions |
|---|---|---|---|---|---|---|---|
| R0 current | 6.2% | 0.7% |  30.4% (n=240) |   0.0% (n=38) |  30.4% (n=240) | 73.6% | 0 |
| R2 anscombe | 2.2% | 2.5% |  89.2% (n=240) |   0.0% (n=38) |   5.4% (n=240) | 19.2% | 0 |
| R3 freeman-tukey | 2.2% | 2.9% |  94.2% (n=240) |   0.0% (n=38) |   4.6% (n=240) | 11.2% | 0 |
| R4 poisson/NB exact | 3.6% | 0.7% |  56.7% (n=240) |   0.0% (n=38) |  10.0% (n=240) | 50.7% | 0 |

### account / all / baseline=4: 311 weeks
| rule | false above | false below | drop→0 | −50% det | +100% det | low edge 0 | contradictions |
|---|---|---|---|---|---|---|---|
| R0 current | 4.2% | 3.9% |  98.1% (n=311) |  58.3% (n=307) |  98.1% (n=311) | 1.9% | 0 |
| R2 anscombe | 2.3% | 3.9% |  99.4% (n=311) |  75.9% (n=307) |  95.5% (n=311) | 0.6% | 0 |
| R3 freeman-tukey | 2.3% | 3.9% |  99.7% (n=311) |  75.9% (n=307) |  95.2% (n=311) | 0.3% | 0 |
| R4 poisson/NB exact | 2.9% | 3.9% |  99.4% (n=311) |  61.6% (n=307) |  96.1% (n=311) | 0.6% | 0 |

### account / all / baseline=5: 311 weeks
| rule | false above | false below | drop→0 | −50% det | +100% det | low edge 0 | contradictions |
|---|---|---|---|---|---|---|---|
| R0 current | 3.9% | 4.2% |  98.1% (n=311) |  52.3% (n=310) |  98.1% (n=311) | 1.9% | 0 |
| R2 anscombe | 2.9% | 5.8% |  99.7% (n=311) |  67.4% (n=310) |  90.7% (n=311) | 0.3% | 0 |
| R3 freeman-tukey | 2.9% | 5.8% |  99.7% (n=311) |  67.4% (n=310) |  90.7% (n=311) | 0.3% | 0 |
| R4 poisson/NB exact | 3.5% | 4.8% |  99.4% (n=311) |  55.5% (n=310) |  94.9% (n=311) | 0.6% | 0 |

### account / all / baseline=6: 311 weeks
| rule | false above | false below | drop→0 | −50% det | +100% det | low edge 0 | contradictions |
|---|---|---|---|---|---|---|---|
| R0 current | 2.9% | 3.9% |  99.4% (n=311) |  53.7% (n=309) |  99.4% (n=311) | 0.6% | 0 |
| R2 anscombe | 1.9% | 4.5% |  99.7% (n=311) |  71.2% (n=309) |  95.2% (n=311) | 0.3% | 0 |
| R3 freeman-tukey | 1.6% | 4.2% | 100.0% (n=311) |  70.9% (n=309) |  95.2% (n=311) | 0.0% | 0 |
| R4 poisson/NB exact | 2.3% | 4.2% |  99.4% (n=311) |  57.6% (n=309) |  96.1% (n=311) | 0.6% | 0 |

### account / all / baseline=8: 311 weeks
| rule | false above | false below | drop→0 | −50% det | +100% det | low edge 0 | contradictions |
|---|---|---|---|---|---|---|---|
| R0 current | 2.6% | 2.6% |  99.4% (n=311) |  52.1% (n=311) |  99.4% (n=311) | 0.6% | 0 |
| R2 anscombe | 1.6% | 3.2% | 100.0% (n=311) |  72.7% (n=311) |  96.8% (n=311) | 0.0% | 0 |
| R3 freeman-tukey | 1.6% | 3.2% | 100.0% (n=311) |  72.7% (n=311) |  96.8% (n=311) | 0.0% | 0 |
| R4 poisson/NB exact | 1.6% | 2.6% | 100.0% (n=311) |  57.6% (n=311) |  97.7% (n=311) | 0.0% | 0 |

### account / all / baseline=genuine 4-7: 76 weeks
| rule | false above | false below | drop→0 | −50% det | +100% det | low edge 0 | contradictions |
|---|---|---|---|---|---|---|---|
| R0 current | 5.3% | 3.9% | 100.0% (n=76) |  57.9% (n=76) | 100.0% (n=76) | 0.0% | 0 |
| R2 anscombe | 5.3% | 3.9% | 100.0% (n=76) |  76.3% (n=76) |  98.7% (n=76) | 0.0% | 0 |
| R3 freeman-tukey | 5.3% | 3.9% | 100.0% (n=76) |  76.3% (n=76) | 100.0% (n=76) | 0.0% | 0 |
| R4 poisson/NB exact | 5.3% | 3.9% | 100.0% (n=76) |  63.2% (n=76) | 100.0% (n=76) | 0.0% | 0 |

### account / call_received / baseline=4: 311 weeks
| rule | false above | false below | drop→0 | −50% det | +100% det | low edge 0 | contradictions |
|---|---|---|---|---|---|---|---|
| R0 current | 3.9% | 3.5% |  93.5% (n=307) |  39.8% (n=266) |  93.5% (n=307) | 7.7% | 0 |
| R2 anscombe | 1.9% | 5.5% |  99.3% (n=307) |  57.1% (n=266) |  78.8% (n=307) | 1.0% | 0 |
| R3 freeman-tukey | 1.6% | 5.5% |  99.3% (n=307) |  57.1% (n=266) |  76.5% (n=307) | 0.6% | 0 |
| R4 poisson/NB exact | 2.3% | 3.9% |  98.4% (n=307) |  43.2% (n=266) |  81.1% (n=307) | 2.9% | 0 |

### account / call_received / baseline=5: 311 weeks
| rule | false above | false below | drop→0 | −50% det | +100% det | low edge 0 | contradictions |
|---|---|---|---|---|---|---|---|
| R0 current | 3.5% | 3.5% |  87.4% (n=309) |  34.6% (n=269) |  87.4% (n=309) | 13.2% | 0 |
| R2 anscombe | 1.6% | 5.5% |  98.4% (n=309) |  50.6% (n=269) |  71.8% (n=309) | 2.3% | 0 |
| R3 freeman-tukey | 1.6% | 5.5% |  98.4% (n=309) |  50.6% (n=269) |  72.2% (n=309) | 2.3% | 0 |
| R4 poisson/NB exact | 2.6% | 3.9% |  96.1% (n=309) |  36.8% (n=269) |  79.6% (n=309) | 4.5% | 0 |

### account / call_received / baseline=6: 311 weeks
| rule | false above | false below | drop→0 | −50% det | +100% det | low edge 0 | contradictions |
|---|---|---|---|---|---|---|---|
| R0 current | 2.9% | 2.6% |  93.2% (n=307) |  37.7% (n=265) |  93.2% (n=307) | 8.0% | 0 |
| R2 anscombe | 1.6% | 4.5% |  99.0% (n=307) |  56.6% (n=265) |  77.9% (n=307) | 2.3% | 0 |
| R3 freeman-tukey | 1.6% | 4.5% |  99.7% (n=307) |  56.6% (n=265) |  76.9% (n=307) | 1.6% | 0 |
| R4 poisson/NB exact | 1.6% | 3.5% |  98.4% (n=307) |  41.9% (n=265) |  82.1% (n=307) | 2.9% | 0 |

### account / call_received / baseline=8: 311 weeks
| rule | false above | false below | drop→0 | −50% det | +100% det | low edge 0 | contradictions |
|---|---|---|---|---|---|---|---|
| R0 current | 2.9% | 1.9% |  93.2% (n=309) |  32.1% (n=271) |  93.2% (n=309) | 7.4% | 0 |
| R2 anscombe | 1.6% | 4.5% |  98.4% (n=309) |  50.9% (n=271) |  77.7% (n=309) | 2.3% | 0 |
| R3 freeman-tukey | 1.6% | 4.5% |  99.7% (n=309) |  50.9% (n=271) |  76.7% (n=309) | 1.0% | 0 |
| R4 poisson/NB exact | 1.6% | 2.6% |  98.1% (n=309) |  36.9% (n=271) |  84.5% (n=309) | 2.6% | 0 |

### account / call_received / baseline=genuine 4-7: 76 weeks
| rule | false above | false below | drop→0 | −50% det | +100% det | low edge 0 | contradictions |
|---|---|---|---|---|---|---|---|
| R0 current | 6.6% | 1.3% |  92.1% (n=76) |  26.6% (n=64) |  92.1% (n=76) | 7.9% | 0 |
| R2 anscombe | 5.3% | 2.6% | 100.0% (n=76) |  53.1% (n=64) |  69.7% (n=76) | 0.0% | 0 |
| R3 freeman-tukey | 5.3% | 2.6% | 100.0% (n=76) |  53.1% (n=64) |  69.7% (n=76) | 0.0% | 0 |
| R4 poisson/NB exact | 6.6% | 1.3% | 100.0% (n=76) |  32.8% (n=64) |  75.0% (n=76) | 0.0% | 0 |

## C. Threshold sensitivity (site level all types + account level all types; full baselines)

### site / all
| rule | param | false above | false below | drop→0 | −50% | +100% |
|---|---|---|---|---|---|---|
| R0 current | k=1.75 | 5.5% | 2.6% |  87.0% (n=1104) |   5.7% (n=770) |  87.0% (n=1104) |
| R0 current | k=2.0 | 3.8% | 1.0% |  77.8% (n=1104) |   0.0% (n=770) |  77.8% (n=1104) |
| R0 current | k=2.5 | 1.3% | 0.1% |  46.7% (n=1104) |   0.0% (n=770) |  46.7% (n=1104) |
| R0 current | k=3.0 | 0.3% | 0.0% |   7.6% (n=1104) |   0.0% (n=770) |   7.6% (n=1104) |
| R2 anscombe | k=1.75 | 2.9% | 5.5% |  99.7% (n=1104) |  43.9% (n=770) |  70.9% (n=1104) |
| R2 anscombe | k=2.0 | 1.4% | 2.9% |  97.6% (n=1104) |   5.7% (n=770) |  48.4% (n=1104) |
| R2 anscombe | k=2.5 | 0.3% | 1.1% |  90.9% (n=1104) |   0.0% (n=770) |   7.7% (n=1104) |
| R2 anscombe | k=3.0 | 0.0% | 0.5% |  79.3% (n=1104) |   0.0% (n=770) |   0.0% (n=1104) |
| R3 freeman-tukey | k=1.75 | 2.8% | 4.4% |  99.9% (n=1104) |  36.0% (n=770) |  70.9% (n=1104) |
| R3 freeman-tukey | k=2.0 | 1.4% | 2.9% |  99.5% (n=1104) |   5.7% (n=770) |  47.6% (n=1104) |
| R3 freeman-tukey | k=2.5 | 0.3% | 1.1% |  95.1% (n=1104) |   0.0% (n=770) |   7.7% (n=1104) |
| R3 freeman-tukey | k=3.0 | 0.0% | 0.5% |  85.3% (n=1104) |   0.0% (n=770) |   0.0% (n=1104) |
| R4 poisson/NB exact | alpha=0.05 | 3.8% | 2.8% |  94.7% (n=1104) |   6.0% (n=770) |  77.8% (n=1104) |
| R4 poisson/NB exact | alpha=0.025 | 1.7% | 1.2% |  88.8% (n=1104) |   0.0% (n=770) |  56.5% (n=1104) |
| R4 poisson/NB exact | alpha=0.01 | 0.4% | 0.5% |  74.7% (n=1104) |   0.0% (n=770) |  22.6% (n=1104) |
| R4 poisson/NB exact | alpha=0.005 | 0.3% | 0.1% |  65.5% (n=1104) |   0.0% (n=770) |   7.6% (n=1104) |

### site / call_received
| rule | param | false above | false below | drop→0 | −50% | +100% |
|---|---|---|---|---|---|---|
| R0 current | k=1.75 | 5.3% | 2.4% |  67.3% (n=967) |   0.0% (n=129) |  67.3% (n=967) |
| R0 current | k=2.0 | 3.2% | 0.8% |  36.5% (n=967) |   0.0% (n=129) |  36.5% (n=967) |
| R0 current | k=2.5 | 1.4% | 0.0% |   3.3% (n=967) |   0.0% (n=129) |   3.3% (n=967) |
| R0 current | k=3.0 | 0.5% | 0.0% |   0.0% (n=967) |   0.0% (n=129) |   0.0% (n=967) |
| R2 anscombe | k=1.75 | 2.5% | 5.9% |  97.8% (n=967) |  14.0% (n=129) |  25.0% (n=967) |
| R2 anscombe | k=2.0 | 1.0% | 3.8% |  95.3% (n=967) |   0.0% (n=129) |   4.4% (n=967) |
| R2 anscombe | k=2.5 | 0.2% | 1.4% |  70.1% (n=967) |   0.0% (n=129) |   0.0% (n=967) |
| R2 anscombe | k=3.0 | 0.1% | 0.5% |  36.1% (n=967) |   0.0% (n=129) |   0.0% (n=967) |
| R3 freeman-tukey | k=1.75 | 2.1% | 5.5% |  99.4% (n=967) |   6.2% (n=129) |  25.0% (n=967) |
| R3 freeman-tukey | k=2.0 | 1.0% | 4.3% |  97.4% (n=967) |   0.0% (n=129) |   3.6% (n=967) |
| R3 freeman-tukey | k=2.5 | 0.2% | 2.1% |  89.1% (n=967) |   0.0% (n=129) |   0.0% (n=967) |
| R3 freeman-tukey | k=3.0 | 0.1% | 0.6% |  56.8% (n=967) |   0.0% (n=129) |   0.0% (n=967) |
| R4 poisson/NB exact | alpha=0.05 | 2.7% | 3.0% |  85.2% (n=967) |   0.0% (n=129) |  36.5% (n=967) |
| R4 poisson/NB exact | alpha=0.025 | 1.5% | 1.0% |  55.8% (n=967) |   0.0% (n=129) |   9.7% (n=967) |
| R4 poisson/NB exact | alpha=0.01 | 0.7% | 0.4% |  26.4% (n=967) |   0.0% (n=129) |   0.0% (n=967) |
| R4 poisson/NB exact | alpha=0.005 | 0.5% | 0.3% |  17.4% (n=967) |   0.0% (n=129) |   0.0% (n=967) |

### account / all
| rule | param | false above | false below | drop→0 | −50% | +100% |
|---|---|---|---|---|---|---|
| R0 current | k=1.75 | 4.2% | 3.9% | 100.0% (n=311) |  72.0% (n=311) | 100.0% (n=311) |
| R0 current | k=2.0 | 2.6% | 2.6% |  99.4% (n=311) |  52.1% (n=311) |  99.4% (n=311) |
| R0 current | k=2.5 | 1.3% | 0.6% |  95.5% (n=311) |  28.0% (n=311) |  95.5% (n=311) |
| R0 current | k=3.0 | 0.6% | 0.0% |  82.3% (n=311) |   8.4% (n=311) |  82.3% (n=311) |
| R2 anscombe | k=1.75 | 2.9% | 5.1% | 100.0% (n=311) |  89.1% (n=311) |  99.4% (n=311) |
| R2 anscombe | k=2.0 | 1.6% | 3.2% | 100.0% (n=311) |  72.7% (n=311) |  96.8% (n=311) |
| R2 anscombe | k=2.5 | 0.6% | 1.3% | 100.0% (n=311) |  44.1% (n=311) |  82.3% (n=311) |
| R2 anscombe | k=3.0 | 0.3% | 0.0% |  99.7% (n=311) |  25.4% (n=311) |  58.8% (n=311) |
| R3 freeman-tukey | k=1.75 | 2.9% | 5.1% | 100.0% (n=311) |  87.8% (n=311) |  99.4% (n=311) |
| R3 freeman-tukey | k=2.0 | 1.6% | 3.2% | 100.0% (n=311) |  72.7% (n=311) |  96.8% (n=311) |
| R3 freeman-tukey | k=2.5 | 0.6% | 1.3% | 100.0% (n=311) |  44.1% (n=311) |  82.3% (n=311) |
| R3 freeman-tukey | k=3.0 | 0.3% | 0.0% | 100.0% (n=311) |  25.4% (n=311) |  58.8% (n=311) |
| R4 poisson/NB exact | alpha=0.05 | 3.9% | 4.5% | 100.0% (n=311) |  79.4% (n=311) |  99.4% (n=311) |
| R4 poisson/NB exact | alpha=0.025 | 1.6% | 2.6% | 100.0% (n=311) |  57.6% (n=311) |  97.7% (n=311) |
| R4 poisson/NB exact | alpha=0.01 | 1.3% | 1.3% |  99.7% (n=311) |  40.8% (n=311) |  88.1% (n=311) |
| R4 poisson/NB exact | alpha=0.005 | 0.6% | 0.6% |  99.7% (n=311) |  30.2% (n=311) |  80.7% (n=311) |

### account / call_received
| rule | param | false above | false below | drop→0 | −50% | +100% |
|---|---|---|---|---|---|---|
| R0 current | k=1.75 | 4.2% | 3.9% |  96.1% (n=309) |  49.4% (n=271) |  96.1% (n=309) |
| R0 current | k=2.0 | 2.9% | 1.9% |  93.2% (n=309) |  32.1% (n=271) |  93.2% (n=309) |
| R0 current | k=2.5 | 1.0% | 0.0% |  75.4% (n=309) |  11.4% (n=271) |  75.4% (n=309) |
| R0 current | k=3.0 | 0.3% | 0.0% |  54.7% (n=309) |   3.0% (n=271) |  54.7% (n=309) |
| R2 anscombe | k=1.75 | 2.9% | 4.8% |  99.7% (n=309) |  75.3% (n=271) |  89.3% (n=309) |
| R2 anscombe | k=2.0 | 1.6% | 4.5% |  98.4% (n=309) |  50.9% (n=271) |  77.7% (n=309) |
| R2 anscombe | k=2.5 | 0.3% | 1.3% |  98.1% (n=309) |  26.9% (n=271) |  54.4% (n=309) |
| R2 anscombe | k=3.0 | 0.3% | 0.0% |  93.5% (n=309) |  11.4% (n=271) |  36.9% (n=309) |
| R3 freeman-tukey | k=1.75 | 2.9% | 4.8% | 100.0% (n=309) |  70.1% (n=271) |  89.3% (n=309) |
| R3 freeman-tukey | k=2.0 | 1.6% | 4.5% |  99.7% (n=309) |  50.9% (n=271) |  76.7% (n=309) |
| R3 freeman-tukey | k=2.5 | 0.3% | 1.3% |  98.1% (n=309) |  26.9% (n=271) |  54.4% (n=309) |
| R3 freeman-tukey | k=3.0 | 0.3% | 0.0% |  96.1% (n=309) |  11.4% (n=271) |  36.9% (n=309) |
| R4 poisson/NB exact | alpha=0.05 | 3.5% | 4.2% |  98.1% (n=309) |  56.8% (n=271) |  93.2% (n=309) |
| R4 poisson/NB exact | alpha=0.025 | 1.6% | 2.6% |  98.1% (n=309) |  36.9% (n=271) |  84.5% (n=309) |
| R4 poisson/NB exact | alpha=0.01 | 0.3% | 0.6% |  91.9% (n=309) |  23.2% (n=271) |  63.4% (n=309) |
| R4 poisson/NB exact | alpha=0.005 | 0.3% | 0.0% |  87.1% (n=309) |  12.2% (n=271) |  53.1% (n=309) |

## D. Spike week (acct 6, 2026-06-01) and post-spike weeks
- R0 current: all: sites above 15/15, total above; call_received: sites above 15/15, total above; lead_created: sites above 15/15, total above; appointment_set: sites above 14/15, total above | post-spike (7 weeks × 16 series × 4 types = 448): status changes vs spike-neutralised baseline 16, max range-edge shift 23
- R1 current+zero: all: sites above 15/15, total above; call_received: sites above 15/15, total above; lead_created: sites above 15/15, total above; appointment_set: sites above 14/15, total above | post-spike (7 weeks × 16 series × 4 types = 448): status changes vs spike-neutralised baseline 16, max range-edge shift 23
- R2 anscombe: all: sites above 15/15, total above; call_received: sites above 15/15, total above; lead_created: sites above 15/15, total above; appointment_set: sites above 13/15, total above | post-spike (7 weeks × 16 series × 4 types = 448): status changes vs spike-neutralised baseline 11, max range-edge shift 32
- R3 freeman-tukey: all: sites above 15/15, total above; call_received: sites above 15/15, total above; lead_created: sites above 15/15, total above; appointment_set: sites above 14/15, total above | post-spike (7 weeks × 16 series × 4 types = 448): status changes vs spike-neutralised baseline 20, max range-edge shift 32
- R4 poisson/NB exact: all: sites above 15/15, total above; call_received: sites above 15/15, total above; lead_created: sites above 15/15, total above; appointment_set: sites above 13/15, total above | post-spike (7 weeks × 16 series × 4 types = 448): status changes vs spike-neutralised baseline 9, max range-edge shift 26

## E. Exhaustive contradiction check (every real baseline with ≥4 eligible weeks, every count 0..high+24)
- R0 current: 0 contradictions in 242549 (baseline,count) pairs
- R1 current+zero: 1426 contradictions in 242549 (baseline,count) pairs
- R2 anscombe: 0 contradictions in 252960 (baseline,count) pairs
- R3 freeman-tukey: 0 contradictions in 252947 (baseline,count) pairs
- R4 poisson/NB exact: 0 contradictions in 249758 (baseline,count) pairs
