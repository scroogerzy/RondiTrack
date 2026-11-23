import 'package:ronditrack_mobile/core/money/cents.dart';

enum PayoutStatus {
  next,
  upcoming,
  paid,
  unknown;

  factory fromApi(String value) => switch (value.toLowerCase()) {
    'next' => PayoutStatus.next,
    'upcoming' => PayoutStatus.upcoming,
    'paid' => PayoutStatus.paid,
    _ => PayoutStatus.unknown,
  };

  String get label => switch (this) {
    PayoutStatus.next => 'Next recipient',
    PayoutStatus.upcoming => 'Upcoming',
    PayoutStatus.paid => 'Paid',
    PayoutStatus.unknown => 'Unknown',
  };
}

class PayoutOrderEntry {
  const new({
    required this.memberId,
    required this.memberName,
    required this.position,
    required this.amount,
    required this.payoutDateUtc,
    required this.status,
  });

  final String memberId;
  final String memberName;
  final int position;
  final Cents amount;
  final DateTime payoutDateUtc;
  final PayoutStatus status;
}

class PayoutOrder {
  const new({
    required this.stokvelId,
    required this.cycleId,
    required this.cycleNumber,
    required this.entries,
  });

  final String stokvelId;
  final String cycleId;
  final int cycleNumber;

  /// Preserves the exact order supplied by the API.
  final List<PayoutOrderEntry> entries;
}
