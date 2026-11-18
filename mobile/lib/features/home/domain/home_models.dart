import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:ronditrack_mobile/core/money/cents.dart';
part 'home_models.freezed.dart';
part 'home_models.g.dart';

@freezed
abstract class StokvelHomeItem with _$StokvelHomeItem {
  const factory({
    required String stokvelId,
    required String name,
    required String role,
    required int? currentCycle,
    @CentsJsonConverter() required Cents amountPaidThisCycleCents,
    @CentsJsonConverter() required Cents amountOwedCents,
    required DateTime? nextPayoutDateUtc,
    @CentsJsonConverter() required Cents arrearsCents,
  }) = _StokvelHomeItem;
  factory fromJson(Map<String, dynamic> json) =>
      _$StokvelHomeItemFromJson(json);
}

@freezed
abstract class HomeResponse with _$HomeResponse {
  const factory({
    required String currentUserId,
    required List<StokvelHomeItem> stokvels,
  }) = _HomeResponse;
  factory fromJson(Map<String, dynamic> json) => _$HomeResponseFromJson(json);
}
