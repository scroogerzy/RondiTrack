// GENERATED CODE - DO NOT MODIFY BY HAND
// coverage:ignore-file
// ignore_for_file: type=lint, type=warning, deprecated_member_use, deprecated_member_use_from_same_package
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'home_models.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// dart format off
T _$identity<T>(T value) => value;

/// @nodoc
mixin _$StokvelHomeItem {

 String get stokvelId; String get name; String get role; int? get currentCycle;@CentsJsonConverter() Cents get amountPaidThisCycleCents;@CentsJsonConverter() Cents get amountOwedCents; DateTime? get nextPayoutDateUtc;@CentsJsonConverter() Cents get arrearsCents;
/// Create a copy of StokvelHomeItem
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$StokvelHomeItemCopyWith<StokvelHomeItem> get copyWith => _$StokvelHomeItemCopyWithImpl<StokvelHomeItem>(this as StokvelHomeItem, _$identity);

  /// Serializes this StokvelHomeItem to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as StokvelHomeItem;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is StokvelHomeItem&&(identical(other.stokvelId, _this.stokvelId) || other.stokvelId == _this.stokvelId)&&(identical(other.name, _this.name) || other.name == _this.name)&&(identical(other.role, _this.role) || other.role == _this.role)&&(identical(other.currentCycle, _this.currentCycle) || other.currentCycle == _this.currentCycle)&&(identical(other.amountPaidThisCycleCents, _this.amountPaidThisCycleCents) || other.amountPaidThisCycleCents == _this.amountPaidThisCycleCents)&&(identical(other.amountOwedCents, _this.amountOwedCents) || other.amountOwedCents == _this.amountOwedCents)&&(identical(other.nextPayoutDateUtc, _this.nextPayoutDateUtc) || other.nextPayoutDateUtc == _this.nextPayoutDateUtc)&&(identical(other.arrearsCents, _this.arrearsCents) || other.arrearsCents == _this.arrearsCents));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as StokvelHomeItem;
  return Object.hash(runtimeType,_this.stokvelId,_this.name,_this.role,_this.currentCycle,_this.amountPaidThisCycleCents,_this.amountOwedCents,_this.nextPayoutDateUtc,_this.arrearsCents);
}

@override
String toString() {
  final _this = this as StokvelHomeItem;
  return 'StokvelHomeItem(stokvelId: ${_this.stokvelId}, name: ${_this.name}, role: ${_this.role}, currentCycle: ${_this.currentCycle}, amountPaidThisCycleCents: ${_this.amountPaidThisCycleCents}, amountOwedCents: ${_this.amountOwedCents}, nextPayoutDateUtc: ${_this.nextPayoutDateUtc}, arrearsCents: ${_this.arrearsCents})';
}


}

/// @nodoc
abstract mixin class $StokvelHomeItemCopyWith<$Res>  {
  factory $StokvelHomeItemCopyWith(StokvelHomeItem value, $Res Function(StokvelHomeItem) _then) = _$StokvelHomeItemCopyWithImpl;
@useResult
$Res call({
 String stokvelId, String name, String role, int? currentCycle,@CentsJsonConverter() Cents amountPaidThisCycleCents,@CentsJsonConverter() Cents amountOwedCents, DateTime? nextPayoutDateUtc,@CentsJsonConverter() Cents arrearsCents
});




}
/// @nodoc
class _$StokvelHomeItemCopyWithImpl<$Res>
    implements $StokvelHomeItemCopyWith<$Res> {
  _$StokvelHomeItemCopyWithImpl(this._self, this._then);

  final StokvelHomeItem _self;
  final $Res Function(StokvelHomeItem) _then;

/// Create a copy of StokvelHomeItem
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? stokvelId = null,Object? name = null,Object? role = null,Object? currentCycle = freezed,Object? amountPaidThisCycleCents = null,Object? amountOwedCents = null,Object? nextPayoutDateUtc = freezed,Object? arrearsCents = null,}) {
  return _then(StokvelHomeItem(
stokvelId: null == stokvelId ? _self.stokvelId : stokvelId // ignore: cast_nullable_to_non_nullable
as String,name: null == name ? _self.name : name // ignore: cast_nullable_to_non_nullable
as String,role: null == role ? _self.role : role // ignore: cast_nullable_to_non_nullable
as String,currentCycle: freezed == currentCycle ? _self.currentCycle : currentCycle // ignore: cast_nullable_to_non_nullable
as int?,amountPaidThisCycleCents: null == amountPaidThisCycleCents ? _self.amountPaidThisCycleCents : amountPaidThisCycleCents // ignore: cast_nullable_to_non_nullable
as Cents,amountOwedCents: null == amountOwedCents ? _self.amountOwedCents : amountOwedCents // ignore: cast_nullable_to_non_nullable
as Cents,nextPayoutDateUtc: freezed == nextPayoutDateUtc ? _self.nextPayoutDateUtc : nextPayoutDateUtc // ignore: cast_nullable_to_non_nullable
as DateTime?,arrearsCents: null == arrearsCents ? _self.arrearsCents : arrearsCents // ignore: cast_nullable_to_non_nullable
as Cents,
  ));
}

}


/// Adds pattern-matching-related methods to [StokvelHomeItem].
extension StokvelHomeItemPatterns on StokvelHomeItem {
/// A variant of `map` that fallback to returning `orElse`.
///
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case final Subclass value:
///     return ...;
///   case _:
///     return orElse();
/// }
/// ```

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _StokvelHomeItem value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _StokvelHomeItem() when $default != null:
return $default(_that);case _:
  return orElse();

}
}
/// A `switch`-like method, using callbacks.
///
/// Callbacks receives the raw object, upcasted.
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case final Subclass value:
///     return ...;
///   case final Subclass2 value:
///     return ...;
/// }
/// ```

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _StokvelHomeItem value)  $default,){
final _that = this;
switch (_that) {
case _StokvelHomeItem():
return $default(_that);case _:
  throw StateError('Unexpected subclass');

}
}
/// A variant of `map` that fallback to returning `null`.
///
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case final Subclass value:
///     return ...;
///   case _:
///     return null;
/// }
/// ```

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _StokvelHomeItem value)?  $default,){
final _that = this;
switch (_that) {
case _StokvelHomeItem() when $default != null:
return $default(_that);case _:
  return null;

}
}
/// A variant of `when` that fallback to an `orElse` callback.
///
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case Subclass(:final field):
///     return ...;
///   case _:
///     return orElse();
/// }
/// ```

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( String stokvelId,  String name,  String role,  int? currentCycle, @CentsJsonConverter()  Cents amountPaidThisCycleCents, @CentsJsonConverter()  Cents amountOwedCents,  DateTime? nextPayoutDateUtc, @CentsJsonConverter()  Cents arrearsCents)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _StokvelHomeItem() when $default != null:
return $default(_that.stokvelId,_that.name,_that.role,_that.currentCycle,_that.amountPaidThisCycleCents,_that.amountOwedCents,_that.nextPayoutDateUtc,_that.arrearsCents);case _:
  return orElse();

}
}
/// A `switch`-like method, using callbacks.
///
/// As opposed to `map`, this offers destructuring.
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case Subclass(:final field):
///     return ...;
///   case Subclass2(:final field2):
///     return ...;
/// }
/// ```

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( String stokvelId,  String name,  String role,  int? currentCycle, @CentsJsonConverter()  Cents amountPaidThisCycleCents, @CentsJsonConverter()  Cents amountOwedCents,  DateTime? nextPayoutDateUtc, @CentsJsonConverter()  Cents arrearsCents)  $default,) {final _that = this;
switch (_that) {
case _StokvelHomeItem():
return $default(_that.stokvelId,_that.name,_that.role,_that.currentCycle,_that.amountPaidThisCycleCents,_that.amountOwedCents,_that.nextPayoutDateUtc,_that.arrearsCents);case _:
  throw StateError('Unexpected subclass');

}
}
/// A variant of `when` that fallback to returning `null`
///
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case Subclass(:final field):
///     return ...;
///   case _:
///     return null;
/// }
/// ```

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( String stokvelId,  String name,  String role,  int? currentCycle, @CentsJsonConverter()  Cents amountPaidThisCycleCents, @CentsJsonConverter()  Cents amountOwedCents,  DateTime? nextPayoutDateUtc, @CentsJsonConverter()  Cents arrearsCents)?  $default,) {final _that = this;
switch (_that) {
case _StokvelHomeItem() when $default != null:
return $default(_that.stokvelId,_that.name,_that.role,_that.currentCycle,_that.amountPaidThisCycleCents,_that.amountOwedCents,_that.nextPayoutDateUtc,_that.arrearsCents);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _StokvelHomeItem implements StokvelHomeItem {
  const _StokvelHomeItem({required this.stokvelId, required this.name, required this.role, required this.currentCycle, @CentsJsonConverter() required this.amountPaidThisCycleCents, @CentsJsonConverter() required this.amountOwedCents, required this.nextPayoutDateUtc, @CentsJsonConverter() required this.arrearsCents});
  factory _StokvelHomeItem.fromJson(Map<String, dynamic> json) => _$StokvelHomeItemFromJson(json);

@override final  String stokvelId;
@override final  String name;
@override final  String role;
@override final  int? currentCycle;
@override@CentsJsonConverter() final  Cents amountPaidThisCycleCents;
@override@CentsJsonConverter() final  Cents amountOwedCents;
@override final  DateTime? nextPayoutDateUtc;
@override@CentsJsonConverter() final  Cents arrearsCents;

/// Create a copy of StokvelHomeItem
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$StokvelHomeItemCopyWith<_StokvelHomeItem> get copyWith => __$StokvelHomeItemCopyWithImpl<_StokvelHomeItem>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$StokvelHomeItemToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _StokvelHomeItem&&(identical(other.stokvelId, stokvelId) || other.stokvelId == stokvelId)&&(identical(other.name, name) || other.name == name)&&(identical(other.role, role) || other.role == role)&&(identical(other.currentCycle, currentCycle) || other.currentCycle == currentCycle)&&(identical(other.amountPaidThisCycleCents, amountPaidThisCycleCents) || other.amountPaidThisCycleCents == amountPaidThisCycleCents)&&(identical(other.amountOwedCents, amountOwedCents) || other.amountOwedCents == amountOwedCents)&&(identical(other.nextPayoutDateUtc, nextPayoutDateUtc) || other.nextPayoutDateUtc == nextPayoutDateUtc)&&(identical(other.arrearsCents, arrearsCents) || other.arrearsCents == arrearsCents));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,stokvelId,name,role,currentCycle,amountPaidThisCycleCents,amountOwedCents,nextPayoutDateUtc,arrearsCents);
}

@override
String toString() {
    return 'StokvelHomeItem(stokvelId: $stokvelId, name: $name, role: $role, currentCycle: $currentCycle, amountPaidThisCycleCents: $amountPaidThisCycleCents, amountOwedCents: $amountOwedCents, nextPayoutDateUtc: $nextPayoutDateUtc, arrearsCents: $arrearsCents)';
}


}

/// @nodoc
abstract mixin class _$StokvelHomeItemCopyWith<$Res> implements $StokvelHomeItemCopyWith<$Res> {
  factory _$StokvelHomeItemCopyWith(_StokvelHomeItem value, $Res Function(_StokvelHomeItem) _then) = __$StokvelHomeItemCopyWithImpl;
@override @useResult
$Res call({
 String stokvelId, String name, String role, int? currentCycle,@CentsJsonConverter() Cents amountPaidThisCycleCents,@CentsJsonConverter() Cents amountOwedCents, DateTime? nextPayoutDateUtc,@CentsJsonConverter() Cents arrearsCents
});




}
/// @nodoc
class __$StokvelHomeItemCopyWithImpl<$Res>
    implements _$StokvelHomeItemCopyWith<$Res> {
  __$StokvelHomeItemCopyWithImpl(this._self, this._then);

  final _StokvelHomeItem _self;
  final $Res Function(_StokvelHomeItem) _then;

/// Create a copy of StokvelHomeItem
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? stokvelId = null,Object? name = null,Object? role = null,Object? currentCycle = freezed,Object? amountPaidThisCycleCents = null,Object? amountOwedCents = null,Object? nextPayoutDateUtc = freezed,Object? arrearsCents = null,}) {
  return _then(_StokvelHomeItem(
stokvelId: null == stokvelId ? _self.stokvelId : stokvelId // ignore: cast_nullable_to_non_nullable
as String,name: null == name ? _self.name : name // ignore: cast_nullable_to_non_nullable
as String,role: null == role ? _self.role : role // ignore: cast_nullable_to_non_nullable
as String,currentCycle: freezed == currentCycle ? _self.currentCycle : currentCycle // ignore: cast_nullable_to_non_nullable
as int?,amountPaidThisCycleCents: null == amountPaidThisCycleCents ? _self.amountPaidThisCycleCents : amountPaidThisCycleCents // ignore: cast_nullable_to_non_nullable
as Cents,amountOwedCents: null == amountOwedCents ? _self.amountOwedCents : amountOwedCents // ignore: cast_nullable_to_non_nullable
as Cents,nextPayoutDateUtc: freezed == nextPayoutDateUtc ? _self.nextPayoutDateUtc : nextPayoutDateUtc // ignore: cast_nullable_to_non_nullable
as DateTime?,arrearsCents: null == arrearsCents ? _self.arrearsCents : arrearsCents // ignore: cast_nullable_to_non_nullable
as Cents,
  ));
}


}


/// @nodoc
mixin _$HomeResponse {

 String get currentUserId; List<StokvelHomeItem> get stokvels;
/// Create a copy of HomeResponse
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$HomeResponseCopyWith<HomeResponse> get copyWith => _$HomeResponseCopyWithImpl<HomeResponse>(this as HomeResponse, _$identity);

  /// Serializes this HomeResponse to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as HomeResponse;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is HomeResponse&&(identical(other.currentUserId, _this.currentUserId) || other.currentUserId == _this.currentUserId)&&const DeepCollectionEquality().equals(other.stokvels, _this.stokvels));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as HomeResponse;
  return Object.hash(runtimeType,_this.currentUserId,const DeepCollectionEquality().hash(_this.stokvels));
}

@override
String toString() {
  final _this = this as HomeResponse;
  return 'HomeResponse(currentUserId: ${_this.currentUserId}, stokvels: ${_this.stokvels})';
}


}

/// @nodoc
abstract mixin class $HomeResponseCopyWith<$Res>  {
  factory $HomeResponseCopyWith(HomeResponse value, $Res Function(HomeResponse) _then) = _$HomeResponseCopyWithImpl;
@useResult
$Res call({
 String currentUserId, List<StokvelHomeItem> stokvels
});




}
/// @nodoc
class _$HomeResponseCopyWithImpl<$Res>
    implements $HomeResponseCopyWith<$Res> {
  _$HomeResponseCopyWithImpl(this._self, this._then);

  final HomeResponse _self;
  final $Res Function(HomeResponse) _then;

/// Create a copy of HomeResponse
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? currentUserId = null,Object? stokvels = null,}) {
  return _then(HomeResponse(
currentUserId: null == currentUserId ? _self.currentUserId : currentUserId // ignore: cast_nullable_to_non_nullable
as String,stokvels: null == stokvels ? _self.stokvels : stokvels // ignore: cast_nullable_to_non_nullable
as List<StokvelHomeItem>,
  ));
}

}


/// Adds pattern-matching-related methods to [HomeResponse].
extension HomeResponsePatterns on HomeResponse {
/// A variant of `map` that fallback to returning `orElse`.
///
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case final Subclass value:
///     return ...;
///   case _:
///     return orElse();
/// }
/// ```

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _HomeResponse value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _HomeResponse() when $default != null:
return $default(_that);case _:
  return orElse();

}
}
/// A `switch`-like method, using callbacks.
///
/// Callbacks receives the raw object, upcasted.
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case final Subclass value:
///     return ...;
///   case final Subclass2 value:
///     return ...;
/// }
/// ```

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _HomeResponse value)  $default,){
final _that = this;
switch (_that) {
case _HomeResponse():
return $default(_that);case _:
  throw StateError('Unexpected subclass');

}
}
/// A variant of `map` that fallback to returning `null`.
///
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case final Subclass value:
///     return ...;
///   case _:
///     return null;
/// }
/// ```

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _HomeResponse value)?  $default,){
final _that = this;
switch (_that) {
case _HomeResponse() when $default != null:
return $default(_that);case _:
  return null;

}
}
/// A variant of `when` that fallback to an `orElse` callback.
///
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case Subclass(:final field):
///     return ...;
///   case _:
///     return orElse();
/// }
/// ```

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( String currentUserId,  List<StokvelHomeItem> stokvels)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _HomeResponse() when $default != null:
return $default(_that.currentUserId,_that.stokvels);case _:
  return orElse();

}
}
/// A `switch`-like method, using callbacks.
///
/// As opposed to `map`, this offers destructuring.
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case Subclass(:final field):
///     return ...;
///   case Subclass2(:final field2):
///     return ...;
/// }
/// ```

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( String currentUserId,  List<StokvelHomeItem> stokvels)  $default,) {final _that = this;
switch (_that) {
case _HomeResponse():
return $default(_that.currentUserId,_that.stokvels);case _:
  throw StateError('Unexpected subclass');

}
}
/// A variant of `when` that fallback to returning `null`
///
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case Subclass(:final field):
///     return ...;
///   case _:
///     return null;
/// }
/// ```

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( String currentUserId,  List<StokvelHomeItem> stokvels)?  $default,) {final _that = this;
switch (_that) {
case _HomeResponse() when $default != null:
return $default(_that.currentUserId,_that.stokvels);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _HomeResponse implements HomeResponse {
  const _HomeResponse({required this.currentUserId, required  List<StokvelHomeItem> stokvels}): _stokvels = stokvels;
  factory _HomeResponse.fromJson(Map<String, dynamic> json) => _$HomeResponseFromJson(json);

@override final  String currentUserId;
 final  List<StokvelHomeItem> _stokvels;
@override List<StokvelHomeItem> get stokvels {
  if (_stokvels is EqualUnmodifiableListView) return _stokvels;
  // ignore: implicit_dynamic_type
  return EqualUnmodifiableListView(_stokvels);
}


/// Create a copy of HomeResponse
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$HomeResponseCopyWith<_HomeResponse> get copyWith => __$HomeResponseCopyWithImpl<_HomeResponse>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$HomeResponseToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _HomeResponse&&(identical(other.currentUserId, currentUserId) || other.currentUserId == currentUserId)&&const DeepCollectionEquality().equals(other.stokvels, _stokvels));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,currentUserId,const DeepCollectionEquality().hash(_stokvels));
}

@override
String toString() {
    return 'HomeResponse(currentUserId: $currentUserId, stokvels: $stokvels)';
}


}

/// @nodoc
abstract mixin class _$HomeResponseCopyWith<$Res> implements $HomeResponseCopyWith<$Res> {
  factory _$HomeResponseCopyWith(_HomeResponse value, $Res Function(_HomeResponse) _then) = __$HomeResponseCopyWithImpl;
@override @useResult
$Res call({
 String currentUserId, List<StokvelHomeItem> stokvels
});




}
/// @nodoc
class __$HomeResponseCopyWithImpl<$Res>
    implements _$HomeResponseCopyWith<$Res> {
  __$HomeResponseCopyWithImpl(this._self, this._then);

  final _HomeResponse _self;
  final $Res Function(_HomeResponse) _then;

/// Create a copy of HomeResponse
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? currentUserId = null,Object? stokvels = null,}) {
  return _then(_HomeResponse(
currentUserId: null == currentUserId ? _self.currentUserId : currentUserId // ignore: cast_nullable_to_non_nullable
as String,stokvels: null == stokvels ? _self._stokvels : stokvels // ignore: cast_nullable_to_non_nullable
as List<StokvelHomeItem>,
  ));
}


}

// dart format on
