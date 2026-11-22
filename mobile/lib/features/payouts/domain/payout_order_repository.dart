// ignore_for_file: lines_longer_than_80_chars
import 'package:ronditrack_mobile/features/payouts/domain/payout_order.dart';

abstract interface class PayoutOrderRepository {
  Future<PayoutOrder?> load(String stokvelId, String cycleId);
}
