import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/money/cents.dart';
import '../../../core/network/failure.dart';
import '../../../core/network/rondi_api.dart';
import '../domain/payout_order.dart';
import '../domain/payout_order_repository.dart';

class ApiPayoutOrderRepository implements PayoutOrderRepository {
  ApiPayoutOrderRepository(this.api);
  final RondiApi api;

  @override
  Future<PayoutOrder?> load(String stokvelId, String cycleId) async {
    try {
      final json = await api.payoutOrder(stokvelId, cycleId);
      final entries = (json['entries'] as List? ?? [])
          .whereType<Map>()
          .map((raw) {
        final item = Map<String, dynamic>.from(raw);
        return PayoutOrderEntry(
          memberId: item['memberId'] as String,
          memberName: item['memberName'] as String,
          position: (item['position'] as num).toInt(),
          amount: Cents((item['amountInCents'] as num).toInt()),
          payoutDateUtc: DateTime.parse(
            item['payoutDateUtc'].toString(),
          ).toUtc(),
          status: PayoutStatus.fromApi(item['status'] as String?),
        );
      }).toList();

      return PayoutOrder(
        stokvelId: json['stokvelId'] as String,
        cycleId: json['cycleId'] as String,
        cycleNumber: (json['cycleNumber'] as num).toInt(),
        entries: entries,
      );
    } catch (error) {
      throw mapFailure(error);
    }
  }
}

final apiPayoutOrderRepositoryProvider = Provider<PayoutOrderRepository>(
  (ref) => ApiPayoutOrderRepository(ref.watch(rondiApiProvider)),
);
