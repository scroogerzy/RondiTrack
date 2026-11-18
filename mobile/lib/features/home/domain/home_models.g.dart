// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'home_models.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_StokvelHomeItem _$StokvelHomeItemFromJson(Map<String, dynamic> json) =>
    _StokvelHomeItem(
      stokvelId: json['stokvelId'] as String,
      name: json['name'] as String,
      role: json['role'] as String,
      currentCycle: (json['currentCycle'] as num?)?.toInt(),
      amountPaidThisCycleCents: const CentsJsonConverter().fromJson(
        (json['amountPaidThisCycleCents'] as num).toInt(),
      ),
      amountOwedCents: const CentsJsonConverter().fromJson(
        (json['amountOwedCents'] as num).toInt(),
      ),
      nextPayoutDateUtc: json['nextPayoutDateUtc'] == null
          ? null
          : DateTime.parse(json['nextPayoutDateUtc'] as String),
      arrearsCents: const CentsJsonConverter().fromJson(
        (json['arrearsCents'] as num).toInt(),
      ),
    );

Map<String, dynamic> _$StokvelHomeItemToJson(_StokvelHomeItem instance) =>
    <String, dynamic>{
      'stokvelId': instance.stokvelId,
      'name': instance.name,
      'role': instance.role,
      'currentCycle': instance.currentCycle,
      'amountPaidThisCycleCents': const CentsJsonConverter().toJson(
        instance.amountPaidThisCycleCents,
      ),
      'amountOwedCents': const CentsJsonConverter().toJson(
        instance.amountOwedCents,
      ),
      'nextPayoutDateUtc': instance.nextPayoutDateUtc?.toIso8601String(),
      'arrearsCents': const CentsJsonConverter().toJson(instance.arrearsCents),
    };

_HomeResponse _$HomeResponseFromJson(Map<String, dynamic> json) =>
    _HomeResponse(
      currentUserId: json['currentUserId'] as String,
      stokvels: (json['stokvels'] as List<dynamic>)
          .map((e) => StokvelHomeItem.fromJson(e as Map<String, dynamic>))
          .toList(),
    );

Map<String, dynamic> _$HomeResponseToJson(_HomeResponse instance) =>
    <String, dynamic>{
      'currentUserId': instance.currentUserId,
      'stokvels': instance.stokvels,
    };
