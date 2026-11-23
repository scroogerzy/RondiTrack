import 'package:flutter_test/flutter_test.dart';
import 'package:ronditrack_mobile/core/money/cents.dart';
import 'package:ronditrack_mobile/features/payouts/domain/payout_order.dart';

void main() {
  group('PayoutStatus', () {
    test('maps known API values and preserves unknown values safely', () {
      expect(PayoutStatus.fromApi('next'), PayoutStatus.next);
      expect(PayoutStatus.fromApi('upcoming'), PayoutStatus.upcoming);
      expect(PayoutStatus.fromApi('paid'), PayoutStatus.paid);
      expect(PayoutStatus.fromApi('future-value'), PayoutStatus.unknown);
    });

    test('has exhaustive human-readable labels', () {
      expect(PayoutStatus.next.label, 'Next recipient');
      expect(PayoutStatus.upcoming.label, 'Upcoming');
      expect(PayoutStatus.paid.label, 'Paid');
      expect(PayoutStatus.unknown.label, 'Unknown');
    });
  });

  test('payout entries retain API order and integer-cent amounts', () {
    final entries = [
      PayoutOrderEntry(
        memberId: 'a',
        memberName: 'A',
        position: 1,
        amount: const Cents(150000),
        payoutDateUtc: DateTime.utc(2026, 11, 30),
        status: PayoutStatus.next,
      ),
      PayoutOrderEntry(
        memberId: 'b',
        memberName: 'B',
        position: 2,
        amount: const Cents(150000),
        payoutDateUtc: DateTime.utc(2026, 11, 30),
        status: PayoutStatus.upcoming,
      ),
    ];
    final order = PayoutOrder(
      stokvelId: 's',
      cycleId: 'c',
      cycleNumber: 5,
      entries: entries,
    );
    expect(order.entries.map((entry) => entry.memberId), ['a', 'b']);
    expect(order.entries.first.amount.value, 150000);
  });
}
