import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/money/cents.dart';
import '../../../core/network/failure.dart';
import '../../../core/network/rondi_api.dart';
import '../domain/home_models.dart';
import '../domain/home_repository.dart';
import '../../stokvel/domain/stokvel_detail.dart';

class ApiHomeRepository implements HomeRepository {
  ApiHomeRepository(this.api);
  final RondiApi api;

  DateTime _instant(dynamic value) =>
      DateTime.parse(value.toString()).toUtc();

  DateTime _date(dynamic value) {
    final parts = value.toString().split('-').map(int.parse).toList();
    return DateTime.utc(parts[0], parts[1], parts[2]);
  }

  int _int(dynamic value) => (value as num?)?.toInt() ?? 0;

  @override
  Future<HomeResponse> loadHome() async {
    try {
      final json = await api.home();
      final items = (json['stokvels'] as List? ?? [])
          .whereType<Map>()
          .map((raw) {
        final item = Map<String, dynamic>.from(raw);
        return StokvelHomeItem(
          stokvelId: item['stokvelId'] as String,
          name: item['name'] as String,
          role: item['role'] as String,
          currentCycle: item['currentCycle'] as int?,
          amountPaidThisCycleCents: Cents(_int(item['amountPaidThisCycleCents'])),
          amountOwedCents: Cents(_int(item['amountOwedCents'])),
          nextPayoutDateUtc: item['nextPayoutDateUtc'] == null
              ? null
              : _instant(item['nextPayoutDateUtc']),
          arrearsCents: Cents(_int(item['arrearsCents'])),
        );
      }).toList();

      return HomeResponse(
        currentUserId: json['currentUserId'] as String,
        stokvels: items,
      );
    } catch (error) {
      throw mapFailure(error);
    }
  }

  @override
  Future<StokvelDetail?> loadStokvel(String stokvelId) async {
    try {
      final home = await loadHome();
      final item = home.stokvels
          .where((entry) => entry.stokvelId == stokvelId)
          .firstOrNull;
      if (item == null) throw const NotFoundFailure();

      final page = await api.cycles(stokvelId);
      final cycles = (page['items'] as List? ?? [])
          .whereType<Map>()
          .map((e) => Map<String, dynamic>.from(e))
          .toList();

      final cycleJson = cycles.cast<Map<String, dynamic>?>().firstWhere(
        (e) => e?['periodNumber'] == item.currentCycle,
        orElse: () => null,
      );

      if (cycleJson == null) {
        return StokvelDetail(
          homeItem: item,
          currentUserId: home.currentUserId,
          currentCycle: null,
          contributions: const [],
        );
      }

      final cycleId = cycleJson['id'] as String;
      final cycle = await api.cycle(stokvelId, cycleId);
      final contributionPage = await api.contributions(stokvelId, cycleId);
      final contributions = (contributionPage['items'] as List? ?? [])
          .whereType<Map>()
          .map((raw) {
        final value = Map<String, dynamic>.from(raw);
        return StokvelContribution(
          id: value['id'] as String,
          userId: value['userId'] as String,
          cycleId: value['cycleId'] as String,
          amountCents: Cents(((value['amount'] as num) * 100).round()),
          recordedAtUtc: _instant(value['recordedAt']),
        );
      }).toList();

      final currentCycle = StokvelCycle(
        id: cycle['id'] as String,
        periodNumber: _int(cycle['periodNumber']),
        startDateUtc: _date(cycle['startDate']),
        endDateUtc: _date(cycle['endDate']),
        targetAmountCents: Cents(((cycle['targetAmount'] as num) * 100).round()),
        status: cycle['status'] as String? ?? '',
        totalCollectedCents: Cents(_int(cycle['totalCollectedCents'])),
        amountDueCents: Cents(_int(cycle['amountDueCents'])),
        closedAtUtc: cycle['closedAtUtc'] == null
            ? null
            : _instant(cycle['closedAtUtc']),
        payoutProcessedAtUtc: cycle['payoutProcessedAtUtc'] == null
            ? null
            : _instant(cycle['payoutProcessedAtUtc']),
      );

      return StokvelDetail(
        homeItem: item,
        currentUserId: home.currentUserId,
        currentCycle: currentCycle,
        contributions: contributions,
      );
    } catch (error) {
      throw mapFailure(error);
    }
  }
}

final apiHomeRepositoryProvider = Provider<HomeRepository>(
  (ref) => ApiHomeRepository(ref.watch(rondiApiProvider)),
);
