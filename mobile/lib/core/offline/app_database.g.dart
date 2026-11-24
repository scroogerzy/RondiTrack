// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'app_database.dart';

// ignore_for_file: type=lint
class $HomeCacheEntriesTable extends HomeCacheEntries
    with TableInfo<$HomeCacheEntriesTable, HomeCacheEntry> {
  @override
  final GeneratedDatabase attachedDatabase;
  final String? _alias;
  $HomeCacheEntriesTable(this.attachedDatabase, [this._alias]);
  static const VerificationMeta _cacheKeyMeta = const VerificationMeta(
    'cacheKey',
  );
  @override
  late final GeneratedColumn<String> cacheKey = GeneratedColumn<String>(
    'cache_key',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _payloadMeta = const VerificationMeta(
    'payload',
  );
  @override
  late final GeneratedColumn<String> payload = GeneratedColumn<String>(
    'payload',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _cachedAtUtcMeta = const VerificationMeta(
    'cachedAtUtc',
  );
  @override
  late final GeneratedColumn<int> cachedAtUtc = GeneratedColumn<int>(
    'cached_at_utc',
    aliasedName,
    false,
    type: DriftSqlType.int,
    requiredDuringInsert: true,
  );
  @override
  List<GeneratedColumn> get $columns => [cacheKey, payload, cachedAtUtc];
  @override
  String get aliasedName => _alias ?? actualTableName;
  @override
  String get actualTableName => $name;
  static const String $name = 'home_cache_entries';
  @override
  VerificationContext validateIntegrity(
    Insertable<HomeCacheEntry> instance, {
    bool isInserting = false,
  }) {
    final context = VerificationContext();
    final data = instance.toColumns(true);
    if (data.containsKey('cache_key')) {
      context.handle(
        _cacheKeyMeta,
        cacheKey.isAcceptableOrUnknown(data['cache_key']!, _cacheKeyMeta),
      );
    } else if (isInserting) {
      context.missing(_cacheKeyMeta);
    }
    if (data.containsKey('payload')) {
      context.handle(
        _payloadMeta,
        payload.isAcceptableOrUnknown(data['payload']!, _payloadMeta),
      );
    } else if (isInserting) {
      context.missing(_payloadMeta);
    }
    if (data.containsKey('cached_at_utc')) {
      context.handle(
        _cachedAtUtcMeta,
        cachedAtUtc.isAcceptableOrUnknown(
          data['cached_at_utc']!,
          _cachedAtUtcMeta,
        ),
      );
    } else if (isInserting) {
      context.missing(_cachedAtUtcMeta);
    }
    return context;
  }

  @override
  Set<GeneratedColumn> get $primaryKey => {cacheKey};
  @override
  HomeCacheEntry map(Map<String, dynamic> data, {String? tablePrefix}) {
    final effectivePrefix = tablePrefix != null ? '$tablePrefix.' : '';
    return HomeCacheEntry(
      cacheKey: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}cache_key'],
      )!,
      payload: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}payload'],
      )!,
      cachedAtUtc: attachedDatabase.typeMapping.read(
        DriftSqlType.int,
        data['${effectivePrefix}cached_at_utc'],
      )!,
    );
  }

  @override
  $HomeCacheEntriesTable createAlias(String alias) {
    return $HomeCacheEntriesTable(attachedDatabase, alias);
  }
}

class HomeCacheEntry extends DataClass implements Insertable<HomeCacheEntry> {
  final String cacheKey;
  final String payload;
  final int cachedAtUtc;
  const HomeCacheEntry({
    required this.cacheKey,
    required this.payload,
    required this.cachedAtUtc,
  });
  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    map['cache_key'] = Variable<String>(cacheKey);
    map['payload'] = Variable<String>(payload);
    map['cached_at_utc'] = Variable<int>(cachedAtUtc);
    return map;
  }

  HomeCacheEntriesCompanion toCompanion(bool nullToAbsent) {
    return HomeCacheEntriesCompanion(
      cacheKey: Value(cacheKey),
      payload: Value(payload),
      cachedAtUtc: Value(cachedAtUtc),
    );
  }

  factory HomeCacheEntry.fromJson(
    Map<String, dynamic> json, {
    ValueSerializer? serializer,
  }) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return HomeCacheEntry(
      cacheKey: serializer.fromJson<String>(json['cacheKey']),
      payload: serializer.fromJson<String>(json['payload']),
      cachedAtUtc: serializer.fromJson<int>(json['cachedAtUtc']),
    );
  }
  @override
  Map<String, dynamic> toJson({ValueSerializer? serializer}) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return <String, dynamic>{
      'cacheKey': serializer.toJson<String>(cacheKey),
      'payload': serializer.toJson<String>(payload),
      'cachedAtUtc': serializer.toJson<int>(cachedAtUtc),
    };
  }

  HomeCacheEntry copyWith({
    String? cacheKey,
    String? payload,
    int? cachedAtUtc,
  }) => HomeCacheEntry(
    cacheKey: cacheKey ?? this.cacheKey,
    payload: payload ?? this.payload,
    cachedAtUtc: cachedAtUtc ?? this.cachedAtUtc,
  );
  HomeCacheEntry copyWithCompanion(HomeCacheEntriesCompanion data) {
    return HomeCacheEntry(
      cacheKey: data.cacheKey.present ? data.cacheKey.value : this.cacheKey,
      payload: data.payload.present ? data.payload.value : this.payload,
      cachedAtUtc: data.cachedAtUtc.present
          ? data.cachedAtUtc.value
          : this.cachedAtUtc,
    );
  }

  @override
  String toString() {
    return (StringBuffer('HomeCacheEntry(')
          ..write('cacheKey: $cacheKey, ')
          ..write('payload: $payload, ')
          ..write('cachedAtUtc: $cachedAtUtc')
          ..write(')'))
        .toString();
  }

  @override
  int get hashCode => Object.hash(cacheKey, payload, cachedAtUtc);
  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      (other is HomeCacheEntry &&
          other.cacheKey == this.cacheKey &&
          other.payload == this.payload &&
          other.cachedAtUtc == this.cachedAtUtc);
}

class HomeCacheEntriesCompanion extends UpdateCompanion<HomeCacheEntry> {
  final Value<String> cacheKey;
  final Value<String> payload;
  final Value<int> cachedAtUtc;
  final Value<int> rowid;
  const HomeCacheEntriesCompanion({
    this.cacheKey = const Value.absent(),
    this.payload = const Value.absent(),
    this.cachedAtUtc = const Value.absent(),
    this.rowid = const Value.absent(),
  });
  HomeCacheEntriesCompanion.insert({
    required String cacheKey,
    required String payload,
    required int cachedAtUtc,
    this.rowid = const Value.absent(),
  }) : cacheKey = Value(cacheKey),
       payload = Value(payload),
       cachedAtUtc = Value(cachedAtUtc);
  static Insertable<HomeCacheEntry> custom({
    Expression<String>? cacheKey,
    Expression<String>? payload,
    Expression<int>? cachedAtUtc,
    Expression<int>? rowid,
  }) {
    return RawValuesInsertable({
      if (cacheKey != null) 'cache_key': cacheKey,
      if (payload != null) 'payload': payload,
      if (cachedAtUtc != null) 'cached_at_utc': cachedAtUtc,
      if (rowid != null) 'rowid': rowid,
    });
  }

  HomeCacheEntriesCompanion copyWith({
    Value<String>? cacheKey,
    Value<String>? payload,
    Value<int>? cachedAtUtc,
    Value<int>? rowid,
  }) {
    return HomeCacheEntriesCompanion(
      cacheKey: cacheKey ?? this.cacheKey,
      payload: payload ?? this.payload,
      cachedAtUtc: cachedAtUtc ?? this.cachedAtUtc,
      rowid: rowid ?? this.rowid,
    );
  }

  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    if (cacheKey.present) {
      map['cache_key'] = Variable<String>(cacheKey.value);
    }
    if (payload.present) {
      map['payload'] = Variable<String>(payload.value);
    }
    if (cachedAtUtc.present) {
      map['cached_at_utc'] = Variable<int>(cachedAtUtc.value);
    }
    if (rowid.present) {
      map['rowid'] = Variable<int>(rowid.value);
    }
    return map;
  }

  @override
  String toString() {
    return (StringBuffer('HomeCacheEntriesCompanion(')
          ..write('cacheKey: $cacheKey, ')
          ..write('payload: $payload, ')
          ..write('cachedAtUtc: $cachedAtUtc, ')
          ..write('rowid: $rowid')
          ..write(')'))
        .toString();
  }
}

class $StokvelDetailCacheEntriesTable extends StokvelDetailCacheEntries
    with TableInfo<$StokvelDetailCacheEntriesTable, StokvelDetailCacheEntry> {
  @override
  final GeneratedDatabase attachedDatabase;
  final String? _alias;
  $StokvelDetailCacheEntriesTable(this.attachedDatabase, [this._alias]);
  static const VerificationMeta _cacheKeyMeta = const VerificationMeta(
    'cacheKey',
  );
  @override
  late final GeneratedColumn<String> cacheKey = GeneratedColumn<String>(
    'cache_key',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _payloadMeta = const VerificationMeta(
    'payload',
  );
  @override
  late final GeneratedColumn<String> payload = GeneratedColumn<String>(
    'payload',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _cachedAtUtcMeta = const VerificationMeta(
    'cachedAtUtc',
  );
  @override
  late final GeneratedColumn<int> cachedAtUtc = GeneratedColumn<int>(
    'cached_at_utc',
    aliasedName,
    false,
    type: DriftSqlType.int,
    requiredDuringInsert: true,
  );
  @override
  List<GeneratedColumn> get $columns => [cacheKey, payload, cachedAtUtc];
  @override
  String get aliasedName => _alias ?? actualTableName;
  @override
  String get actualTableName => $name;
  static const String $name = 'stokvel_detail_cache_entries';
  @override
  VerificationContext validateIntegrity(
    Insertable<StokvelDetailCacheEntry> instance, {
    bool isInserting = false,
  }) {
    final context = VerificationContext();
    final data = instance.toColumns(true);
    if (data.containsKey('cache_key')) {
      context.handle(
        _cacheKeyMeta,
        cacheKey.isAcceptableOrUnknown(data['cache_key']!, _cacheKeyMeta),
      );
    } else if (isInserting) {
      context.missing(_cacheKeyMeta);
    }
    if (data.containsKey('payload')) {
      context.handle(
        _payloadMeta,
        payload.isAcceptableOrUnknown(data['payload']!, _payloadMeta),
      );
    } else if (isInserting) {
      context.missing(_payloadMeta);
    }
    if (data.containsKey('cached_at_utc')) {
      context.handle(
        _cachedAtUtcMeta,
        cachedAtUtc.isAcceptableOrUnknown(
          data['cached_at_utc']!,
          _cachedAtUtcMeta,
        ),
      );
    } else if (isInserting) {
      context.missing(_cachedAtUtcMeta);
    }
    return context;
  }

  @override
  Set<GeneratedColumn> get $primaryKey => {cacheKey};
  @override
  StokvelDetailCacheEntry map(
    Map<String, dynamic> data, {
    String? tablePrefix,
  }) {
    final effectivePrefix = tablePrefix != null ? '$tablePrefix.' : '';
    return StokvelDetailCacheEntry(
      cacheKey: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}cache_key'],
      )!,
      payload: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}payload'],
      )!,
      cachedAtUtc: attachedDatabase.typeMapping.read(
        DriftSqlType.int,
        data['${effectivePrefix}cached_at_utc'],
      )!,
    );
  }

  @override
  $StokvelDetailCacheEntriesTable createAlias(String alias) {
    return $StokvelDetailCacheEntriesTable(attachedDatabase, alias);
  }
}

class StokvelDetailCacheEntry extends DataClass
    implements Insertable<StokvelDetailCacheEntry> {
  final String cacheKey;
  final String payload;
  final int cachedAtUtc;
  const StokvelDetailCacheEntry({
    required this.cacheKey,
    required this.payload,
    required this.cachedAtUtc,
  });
  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    map['cache_key'] = Variable<String>(cacheKey);
    map['payload'] = Variable<String>(payload);
    map['cached_at_utc'] = Variable<int>(cachedAtUtc);
    return map;
  }

  StokvelDetailCacheEntriesCompanion toCompanion(bool nullToAbsent) {
    return StokvelDetailCacheEntriesCompanion(
      cacheKey: Value(cacheKey),
      payload: Value(payload),
      cachedAtUtc: Value(cachedAtUtc),
    );
  }

  factory StokvelDetailCacheEntry.fromJson(
    Map<String, dynamic> json, {
    ValueSerializer? serializer,
  }) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return StokvelDetailCacheEntry(
      cacheKey: serializer.fromJson<String>(json['cacheKey']),
      payload: serializer.fromJson<String>(json['payload']),
      cachedAtUtc: serializer.fromJson<int>(json['cachedAtUtc']),
    );
  }
  @override
  Map<String, dynamic> toJson({ValueSerializer? serializer}) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return <String, dynamic>{
      'cacheKey': serializer.toJson<String>(cacheKey),
      'payload': serializer.toJson<String>(payload),
      'cachedAtUtc': serializer.toJson<int>(cachedAtUtc),
    };
  }

  StokvelDetailCacheEntry copyWith({
    String? cacheKey,
    String? payload,
    int? cachedAtUtc,
  }) => StokvelDetailCacheEntry(
    cacheKey: cacheKey ?? this.cacheKey,
    payload: payload ?? this.payload,
    cachedAtUtc: cachedAtUtc ?? this.cachedAtUtc,
  );
  StokvelDetailCacheEntry copyWithCompanion(
    StokvelDetailCacheEntriesCompanion data,
  ) {
    return StokvelDetailCacheEntry(
      cacheKey: data.cacheKey.present ? data.cacheKey.value : this.cacheKey,
      payload: data.payload.present ? data.payload.value : this.payload,
      cachedAtUtc: data.cachedAtUtc.present
          ? data.cachedAtUtc.value
          : this.cachedAtUtc,
    );
  }

  @override
  String toString() {
    return (StringBuffer('StokvelDetailCacheEntry(')
          ..write('cacheKey: $cacheKey, ')
          ..write('payload: $payload, ')
          ..write('cachedAtUtc: $cachedAtUtc')
          ..write(')'))
        .toString();
  }

  @override
  int get hashCode => Object.hash(cacheKey, payload, cachedAtUtc);
  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      (other is StokvelDetailCacheEntry &&
          other.cacheKey == this.cacheKey &&
          other.payload == this.payload &&
          other.cachedAtUtc == this.cachedAtUtc);
}

class StokvelDetailCacheEntriesCompanion
    extends UpdateCompanion<StokvelDetailCacheEntry> {
  final Value<String> cacheKey;
  final Value<String> payload;
  final Value<int> cachedAtUtc;
  final Value<int> rowid;
  const StokvelDetailCacheEntriesCompanion({
    this.cacheKey = const Value.absent(),
    this.payload = const Value.absent(),
    this.cachedAtUtc = const Value.absent(),
    this.rowid = const Value.absent(),
  });
  StokvelDetailCacheEntriesCompanion.insert({
    required String cacheKey,
    required String payload,
    required int cachedAtUtc,
    this.rowid = const Value.absent(),
  }) : cacheKey = Value(cacheKey),
       payload = Value(payload),
       cachedAtUtc = Value(cachedAtUtc);
  static Insertable<StokvelDetailCacheEntry> custom({
    Expression<String>? cacheKey,
    Expression<String>? payload,
    Expression<int>? cachedAtUtc,
    Expression<int>? rowid,
  }) {
    return RawValuesInsertable({
      if (cacheKey != null) 'cache_key': cacheKey,
      if (payload != null) 'payload': payload,
      if (cachedAtUtc != null) 'cached_at_utc': cachedAtUtc,
      if (rowid != null) 'rowid': rowid,
    });
  }

  StokvelDetailCacheEntriesCompanion copyWith({
    Value<String>? cacheKey,
    Value<String>? payload,
    Value<int>? cachedAtUtc,
    Value<int>? rowid,
  }) {
    return StokvelDetailCacheEntriesCompanion(
      cacheKey: cacheKey ?? this.cacheKey,
      payload: payload ?? this.payload,
      cachedAtUtc: cachedAtUtc ?? this.cachedAtUtc,
      rowid: rowid ?? this.rowid,
    );
  }

  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    if (cacheKey.present) {
      map['cache_key'] = Variable<String>(cacheKey.value);
    }
    if (payload.present) {
      map['payload'] = Variable<String>(payload.value);
    }
    if (cachedAtUtc.present) {
      map['cached_at_utc'] = Variable<int>(cachedAtUtc.value);
    }
    if (rowid.present) {
      map['rowid'] = Variable<int>(rowid.value);
    }
    return map;
  }

  @override
  String toString() {
    return (StringBuffer('StokvelDetailCacheEntriesCompanion(')
          ..write('cacheKey: $cacheKey, ')
          ..write('payload: $payload, ')
          ..write('cachedAtUtc: $cachedAtUtc, ')
          ..write('rowid: $rowid')
          ..write(')'))
        .toString();
  }
}

class $CycleCacheEntriesTable extends CycleCacheEntries
    with TableInfo<$CycleCacheEntriesTable, CycleCacheEntry> {
  @override
  final GeneratedDatabase attachedDatabase;
  final String? _alias;
  $CycleCacheEntriesTable(this.attachedDatabase, [this._alias]);
  static const VerificationMeta _cacheKeyMeta = const VerificationMeta(
    'cacheKey',
  );
  @override
  late final GeneratedColumn<String> cacheKey = GeneratedColumn<String>(
    'cache_key',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _payloadMeta = const VerificationMeta(
    'payload',
  );
  @override
  late final GeneratedColumn<String> payload = GeneratedColumn<String>(
    'payload',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _cachedAtUtcMeta = const VerificationMeta(
    'cachedAtUtc',
  );
  @override
  late final GeneratedColumn<int> cachedAtUtc = GeneratedColumn<int>(
    'cached_at_utc',
    aliasedName,
    false,
    type: DriftSqlType.int,
    requiredDuringInsert: true,
  );
  @override
  List<GeneratedColumn> get $columns => [cacheKey, payload, cachedAtUtc];
  @override
  String get aliasedName => _alias ?? actualTableName;
  @override
  String get actualTableName => $name;
  static const String $name = 'cycle_cache_entries';
  @override
  VerificationContext validateIntegrity(
    Insertable<CycleCacheEntry> instance, {
    bool isInserting = false,
  }) {
    final context = VerificationContext();
    final data = instance.toColumns(true);
    if (data.containsKey('cache_key')) {
      context.handle(
        _cacheKeyMeta,
        cacheKey.isAcceptableOrUnknown(data['cache_key']!, _cacheKeyMeta),
      );
    } else if (isInserting) {
      context.missing(_cacheKeyMeta);
    }
    if (data.containsKey('payload')) {
      context.handle(
        _payloadMeta,
        payload.isAcceptableOrUnknown(data['payload']!, _payloadMeta),
      );
    } else if (isInserting) {
      context.missing(_payloadMeta);
    }
    if (data.containsKey('cached_at_utc')) {
      context.handle(
        _cachedAtUtcMeta,
        cachedAtUtc.isAcceptableOrUnknown(
          data['cached_at_utc']!,
          _cachedAtUtcMeta,
        ),
      );
    } else if (isInserting) {
      context.missing(_cachedAtUtcMeta);
    }
    return context;
  }

  @override
  Set<GeneratedColumn> get $primaryKey => {cacheKey};
  @override
  CycleCacheEntry map(Map<String, dynamic> data, {String? tablePrefix}) {
    final effectivePrefix = tablePrefix != null ? '$tablePrefix.' : '';
    return CycleCacheEntry(
      cacheKey: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}cache_key'],
      )!,
      payload: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}payload'],
      )!,
      cachedAtUtc: attachedDatabase.typeMapping.read(
        DriftSqlType.int,
        data['${effectivePrefix}cached_at_utc'],
      )!,
    );
  }

  @override
  $CycleCacheEntriesTable createAlias(String alias) {
    return $CycleCacheEntriesTable(attachedDatabase, alias);
  }
}

class CycleCacheEntry extends DataClass implements Insertable<CycleCacheEntry> {
  final String cacheKey;
  final String payload;
  final int cachedAtUtc;
  const CycleCacheEntry({
    required this.cacheKey,
    required this.payload,
    required this.cachedAtUtc,
  });
  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    map['cache_key'] = Variable<String>(cacheKey);
    map['payload'] = Variable<String>(payload);
    map['cached_at_utc'] = Variable<int>(cachedAtUtc);
    return map;
  }

  CycleCacheEntriesCompanion toCompanion(bool nullToAbsent) {
    return CycleCacheEntriesCompanion(
      cacheKey: Value(cacheKey),
      payload: Value(payload),
      cachedAtUtc: Value(cachedAtUtc),
    );
  }

  factory CycleCacheEntry.fromJson(
    Map<String, dynamic> json, {
    ValueSerializer? serializer,
  }) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return CycleCacheEntry(
      cacheKey: serializer.fromJson<String>(json['cacheKey']),
      payload: serializer.fromJson<String>(json['payload']),
      cachedAtUtc: serializer.fromJson<int>(json['cachedAtUtc']),
    );
  }
  @override
  Map<String, dynamic> toJson({ValueSerializer? serializer}) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return <String, dynamic>{
      'cacheKey': serializer.toJson<String>(cacheKey),
      'payload': serializer.toJson<String>(payload),
      'cachedAtUtc': serializer.toJson<int>(cachedAtUtc),
    };
  }

  CycleCacheEntry copyWith({
    String? cacheKey,
    String? payload,
    int? cachedAtUtc,
  }) => CycleCacheEntry(
    cacheKey: cacheKey ?? this.cacheKey,
    payload: payload ?? this.payload,
    cachedAtUtc: cachedAtUtc ?? this.cachedAtUtc,
  );
  CycleCacheEntry copyWithCompanion(CycleCacheEntriesCompanion data) {
    return CycleCacheEntry(
      cacheKey: data.cacheKey.present ? data.cacheKey.value : this.cacheKey,
      payload: data.payload.present ? data.payload.value : this.payload,
      cachedAtUtc: data.cachedAtUtc.present
          ? data.cachedAtUtc.value
          : this.cachedAtUtc,
    );
  }

  @override
  String toString() {
    return (StringBuffer('CycleCacheEntry(')
          ..write('cacheKey: $cacheKey, ')
          ..write('payload: $payload, ')
          ..write('cachedAtUtc: $cachedAtUtc')
          ..write(')'))
        .toString();
  }

  @override
  int get hashCode => Object.hash(cacheKey, payload, cachedAtUtc);
  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      (other is CycleCacheEntry &&
          other.cacheKey == this.cacheKey &&
          other.payload == this.payload &&
          other.cachedAtUtc == this.cachedAtUtc);
}

class CycleCacheEntriesCompanion extends UpdateCompanion<CycleCacheEntry> {
  final Value<String> cacheKey;
  final Value<String> payload;
  final Value<int> cachedAtUtc;
  final Value<int> rowid;
  const CycleCacheEntriesCompanion({
    this.cacheKey = const Value.absent(),
    this.payload = const Value.absent(),
    this.cachedAtUtc = const Value.absent(),
    this.rowid = const Value.absent(),
  });
  CycleCacheEntriesCompanion.insert({
    required String cacheKey,
    required String payload,
    required int cachedAtUtc,
    this.rowid = const Value.absent(),
  }) : cacheKey = Value(cacheKey),
       payload = Value(payload),
       cachedAtUtc = Value(cachedAtUtc);
  static Insertable<CycleCacheEntry> custom({
    Expression<String>? cacheKey,
    Expression<String>? payload,
    Expression<int>? cachedAtUtc,
    Expression<int>? rowid,
  }) {
    return RawValuesInsertable({
      if (cacheKey != null) 'cache_key': cacheKey,
      if (payload != null) 'payload': payload,
      if (cachedAtUtc != null) 'cached_at_utc': cachedAtUtc,
      if (rowid != null) 'rowid': rowid,
    });
  }

  CycleCacheEntriesCompanion copyWith({
    Value<String>? cacheKey,
    Value<String>? payload,
    Value<int>? cachedAtUtc,
    Value<int>? rowid,
  }) {
    return CycleCacheEntriesCompanion(
      cacheKey: cacheKey ?? this.cacheKey,
      payload: payload ?? this.payload,
      cachedAtUtc: cachedAtUtc ?? this.cachedAtUtc,
      rowid: rowid ?? this.rowid,
    );
  }

  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    if (cacheKey.present) {
      map['cache_key'] = Variable<String>(cacheKey.value);
    }
    if (payload.present) {
      map['payload'] = Variable<String>(payload.value);
    }
    if (cachedAtUtc.present) {
      map['cached_at_utc'] = Variable<int>(cachedAtUtc.value);
    }
    if (rowid.present) {
      map['rowid'] = Variable<int>(rowid.value);
    }
    return map;
  }

  @override
  String toString() {
    return (StringBuffer('CycleCacheEntriesCompanion(')
          ..write('cacheKey: $cacheKey, ')
          ..write('payload: $payload, ')
          ..write('cachedAtUtc: $cachedAtUtc, ')
          ..write('rowid: $rowid')
          ..write(')'))
        .toString();
  }
}

class $PaymentOutboxTable extends PaymentOutbox
    with TableInfo<$PaymentOutboxTable, PaymentOutboxData> {
  @override
  final GeneratedDatabase attachedDatabase;
  final String? _alias;
  $PaymentOutboxTable(this.attachedDatabase, [this._alias]);
  static const VerificationMeta _idempotencyKeyMeta = const VerificationMeta(
    'idempotencyKey',
  );
  @override
  late final GeneratedColumn<String> idempotencyKey = GeneratedColumn<String>(
    'idempotency_key',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _userIdMeta = const VerificationMeta('userId');
  @override
  late final GeneratedColumn<String> userId = GeneratedColumn<String>(
    'user_id',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _stokvelIdMeta = const VerificationMeta(
    'stokvelId',
  );
  @override
  late final GeneratedColumn<String> stokvelId = GeneratedColumn<String>(
    'stokvel_id',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _cycleIdMeta = const VerificationMeta(
    'cycleId',
  );
  @override
  late final GeneratedColumn<String> cycleId = GeneratedColumn<String>(
    'cycle_id',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _amountInCentsMeta = const VerificationMeta(
    'amountInCents',
  );
  @override
  late final GeneratedColumn<int> amountInCents = GeneratedColumn<int>(
    'amount_in_cents',
    aliasedName,
    false,
    type: DriftSqlType.int,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _createdAtUtcMeta = const VerificationMeta(
    'createdAtUtc',
  );
  @override
  late final GeneratedColumn<int> createdAtUtc = GeneratedColumn<int>(
    'created_at_utc',
    aliasedName,
    false,
    type: DriftSqlType.int,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _lastAttemptAtUtcMeta = const VerificationMeta(
    'lastAttemptAtUtc',
  );
  @override
  late final GeneratedColumn<int> lastAttemptAtUtc = GeneratedColumn<int>(
    'last_attempt_at_utc',
    aliasedName,
    true,
    type: DriftSqlType.int,
    requiredDuringInsert: false,
  );
  static const VerificationMeta _attemptCountMeta = const VerificationMeta(
    'attemptCount',
  );
  @override
  late final GeneratedColumn<int> attemptCount = GeneratedColumn<int>(
    'attempt_count',
    aliasedName,
    false,
    type: DriftSqlType.int,
    requiredDuringInsert: false,
    defaultValue: const Constant(0),
  );
  static const VerificationMeta _statusMeta = const VerificationMeta('status');
  @override
  late final GeneratedColumn<String> status = GeneratedColumn<String>(
    'status',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: false,
    defaultValue: const Constant('pending'),
  );
  @override
  List<GeneratedColumn> get $columns => [
    idempotencyKey,
    userId,
    stokvelId,
    cycleId,
    amountInCents,
    createdAtUtc,
    lastAttemptAtUtc,
    attemptCount,
    status,
  ];
  @override
  String get aliasedName => _alias ?? actualTableName;
  @override
  String get actualTableName => $name;
  static const String $name = 'payment_outbox';
  @override
  VerificationContext validateIntegrity(
    Insertable<PaymentOutboxData> instance, {
    bool isInserting = false,
  }) {
    final context = VerificationContext();
    final data = instance.toColumns(true);
    if (data.containsKey('idempotency_key')) {
      context.handle(
        _idempotencyKeyMeta,
        idempotencyKey.isAcceptableOrUnknown(
          data['idempotency_key']!,
          _idempotencyKeyMeta,
        ),
      );
    } else if (isInserting) {
      context.missing(_idempotencyKeyMeta);
    }
    if (data.containsKey('user_id')) {
      context.handle(
        _userIdMeta,
        userId.isAcceptableOrUnknown(data['user_id']!, _userIdMeta),
      );
    } else if (isInserting) {
      context.missing(_userIdMeta);
    }
    if (data.containsKey('stokvel_id')) {
      context.handle(
        _stokvelIdMeta,
        stokvelId.isAcceptableOrUnknown(data['stokvel_id']!, _stokvelIdMeta),
      );
    } else if (isInserting) {
      context.missing(_stokvelIdMeta);
    }
    if (data.containsKey('cycle_id')) {
      context.handle(
        _cycleIdMeta,
        cycleId.isAcceptableOrUnknown(data['cycle_id']!, _cycleIdMeta),
      );
    } else if (isInserting) {
      context.missing(_cycleIdMeta);
    }
    if (data.containsKey('amount_in_cents')) {
      context.handle(
        _amountInCentsMeta,
        amountInCents.isAcceptableOrUnknown(
          data['amount_in_cents']!,
          _amountInCentsMeta,
        ),
      );
    } else if (isInserting) {
      context.missing(_amountInCentsMeta);
    }
    if (data.containsKey('created_at_utc')) {
      context.handle(
        _createdAtUtcMeta,
        createdAtUtc.isAcceptableOrUnknown(
          data['created_at_utc']!,
          _createdAtUtcMeta,
        ),
      );
    } else if (isInserting) {
      context.missing(_createdAtUtcMeta);
    }
    if (data.containsKey('last_attempt_at_utc')) {
      context.handle(
        _lastAttemptAtUtcMeta,
        lastAttemptAtUtc.isAcceptableOrUnknown(
          data['last_attempt_at_utc']!,
          _lastAttemptAtUtcMeta,
        ),
      );
    }
    if (data.containsKey('attempt_count')) {
      context.handle(
        _attemptCountMeta,
        attemptCount.isAcceptableOrUnknown(
          data['attempt_count']!,
          _attemptCountMeta,
        ),
      );
    }
    if (data.containsKey('status')) {
      context.handle(
        _statusMeta,
        status.isAcceptableOrUnknown(data['status']!, _statusMeta),
      );
    }
    return context;
  }

  @override
  Set<GeneratedColumn> get $primaryKey => {idempotencyKey};
  @override
  PaymentOutboxData map(Map<String, dynamic> data, {String? tablePrefix}) {
    final effectivePrefix = tablePrefix != null ? '$tablePrefix.' : '';
    return PaymentOutboxData(
      idempotencyKey: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}idempotency_key'],
      )!,
      userId: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}user_id'],
      )!,
      stokvelId: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}stokvel_id'],
      )!,
      cycleId: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}cycle_id'],
      )!,
      amountInCents: attachedDatabase.typeMapping.read(
        DriftSqlType.int,
        data['${effectivePrefix}amount_in_cents'],
      )!,
      createdAtUtc: attachedDatabase.typeMapping.read(
        DriftSqlType.int,
        data['${effectivePrefix}created_at_utc'],
      )!,
      lastAttemptAtUtc: attachedDatabase.typeMapping.read(
        DriftSqlType.int,
        data['${effectivePrefix}last_attempt_at_utc'],
      ),
      attemptCount: attachedDatabase.typeMapping.read(
        DriftSqlType.int,
        data['${effectivePrefix}attempt_count'],
      )!,
      status: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}status'],
      )!,
    );
  }

  @override
  $PaymentOutboxTable createAlias(String alias) {
    return $PaymentOutboxTable(attachedDatabase, alias);
  }
}

class PaymentOutboxData extends DataClass
    implements Insertable<PaymentOutboxData> {
  final String idempotencyKey;
  final String userId;
  final String stokvelId;
  final String cycleId;
  final int amountInCents;
  final int createdAtUtc;
  final int? lastAttemptAtUtc;
  final int attemptCount;
  final String status;
  const PaymentOutboxData({
    required this.idempotencyKey,
    required this.userId,
    required this.stokvelId,
    required this.cycleId,
    required this.amountInCents,
    required this.createdAtUtc,
    this.lastAttemptAtUtc,
    required this.attemptCount,
    required this.status,
  });
  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    map['idempotency_key'] = Variable<String>(idempotencyKey);
    map['user_id'] = Variable<String>(userId);
    map['stokvel_id'] = Variable<String>(stokvelId);
    map['cycle_id'] = Variable<String>(cycleId);
    map['amount_in_cents'] = Variable<int>(amountInCents);
    map['created_at_utc'] = Variable<int>(createdAtUtc);
    if (!nullToAbsent || lastAttemptAtUtc != null) {
      map['last_attempt_at_utc'] = Variable<int>(lastAttemptAtUtc);
    }
    map['attempt_count'] = Variable<int>(attemptCount);
    map['status'] = Variable<String>(status);
    return map;
  }

  PaymentOutboxCompanion toCompanion(bool nullToAbsent) {
    return PaymentOutboxCompanion(
      idempotencyKey: Value(idempotencyKey),
      userId: Value(userId),
      stokvelId: Value(stokvelId),
      cycleId: Value(cycleId),
      amountInCents: Value(amountInCents),
      createdAtUtc: Value(createdAtUtc),
      lastAttemptAtUtc: lastAttemptAtUtc == null && nullToAbsent
          ? const Value.absent()
          : Value(lastAttemptAtUtc),
      attemptCount: Value(attemptCount),
      status: Value(status),
    );
  }

  factory PaymentOutboxData.fromJson(
    Map<String, dynamic> json, {
    ValueSerializer? serializer,
  }) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return PaymentOutboxData(
      idempotencyKey: serializer.fromJson<String>(json['idempotencyKey']),
      userId: serializer.fromJson<String>(json['userId']),
      stokvelId: serializer.fromJson<String>(json['stokvelId']),
      cycleId: serializer.fromJson<String>(json['cycleId']),
      amountInCents: serializer.fromJson<int>(json['amountInCents']),
      createdAtUtc: serializer.fromJson<int>(json['createdAtUtc']),
      lastAttemptAtUtc: serializer.fromJson<int?>(json['lastAttemptAtUtc']),
      attemptCount: serializer.fromJson<int>(json['attemptCount']),
      status: serializer.fromJson<String>(json['status']),
    );
  }
  @override
  Map<String, dynamic> toJson({ValueSerializer? serializer}) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return <String, dynamic>{
      'idempotencyKey': serializer.toJson<String>(idempotencyKey),
      'userId': serializer.toJson<String>(userId),
      'stokvelId': serializer.toJson<String>(stokvelId),
      'cycleId': serializer.toJson<String>(cycleId),
      'amountInCents': serializer.toJson<int>(amountInCents),
      'createdAtUtc': serializer.toJson<int>(createdAtUtc),
      'lastAttemptAtUtc': serializer.toJson<int?>(lastAttemptAtUtc),
      'attemptCount': serializer.toJson<int>(attemptCount),
      'status': serializer.toJson<String>(status),
    };
  }

  PaymentOutboxData copyWith({
    String? idempotencyKey,
    String? userId,
    String? stokvelId,
    String? cycleId,
    int? amountInCents,
    int? createdAtUtc,
    Value<int?> lastAttemptAtUtc = const Value.absent(),
    int? attemptCount,
    String? status,
  }) => PaymentOutboxData(
    idempotencyKey: idempotencyKey ?? this.idempotencyKey,
    userId: userId ?? this.userId,
    stokvelId: stokvelId ?? this.stokvelId,
    cycleId: cycleId ?? this.cycleId,
    amountInCents: amountInCents ?? this.amountInCents,
    createdAtUtc: createdAtUtc ?? this.createdAtUtc,
    lastAttemptAtUtc: lastAttemptAtUtc.present
        ? lastAttemptAtUtc.value
        : this.lastAttemptAtUtc,
    attemptCount: attemptCount ?? this.attemptCount,
    status: status ?? this.status,
  );
  PaymentOutboxData copyWithCompanion(PaymentOutboxCompanion data) {
    return PaymentOutboxData(
      idempotencyKey: data.idempotencyKey.present
          ? data.idempotencyKey.value
          : this.idempotencyKey,
      userId: data.userId.present ? data.userId.value : this.userId,
      stokvelId: data.stokvelId.present ? data.stokvelId.value : this.stokvelId,
      cycleId: data.cycleId.present ? data.cycleId.value : this.cycleId,
      amountInCents: data.amountInCents.present
          ? data.amountInCents.value
          : this.amountInCents,
      createdAtUtc: data.createdAtUtc.present
          ? data.createdAtUtc.value
          : this.createdAtUtc,
      lastAttemptAtUtc: data.lastAttemptAtUtc.present
          ? data.lastAttemptAtUtc.value
          : this.lastAttemptAtUtc,
      attemptCount: data.attemptCount.present
          ? data.attemptCount.value
          : this.attemptCount,
      status: data.status.present ? data.status.value : this.status,
    );
  }

  @override
  String toString() {
    return (StringBuffer('PaymentOutboxData(')
          ..write('idempotencyKey: $idempotencyKey, ')
          ..write('userId: $userId, ')
          ..write('stokvelId: $stokvelId, ')
          ..write('cycleId: $cycleId, ')
          ..write('amountInCents: $amountInCents, ')
          ..write('createdAtUtc: $createdAtUtc, ')
          ..write('lastAttemptAtUtc: $lastAttemptAtUtc, ')
          ..write('attemptCount: $attemptCount, ')
          ..write('status: $status')
          ..write(')'))
        .toString();
  }

  @override
  int get hashCode => Object.hash(
    idempotencyKey,
    userId,
    stokvelId,
    cycleId,
    amountInCents,
    createdAtUtc,
    lastAttemptAtUtc,
    attemptCount,
    status,
  );
  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      (other is PaymentOutboxData &&
          other.idempotencyKey == this.idempotencyKey &&
          other.userId == this.userId &&
          other.stokvelId == this.stokvelId &&
          other.cycleId == this.cycleId &&
          other.amountInCents == this.amountInCents &&
          other.createdAtUtc == this.createdAtUtc &&
          other.lastAttemptAtUtc == this.lastAttemptAtUtc &&
          other.attemptCount == this.attemptCount &&
          other.status == this.status);
}

class PaymentOutboxCompanion extends UpdateCompanion<PaymentOutboxData> {
  final Value<String> idempotencyKey;
  final Value<String> userId;
  final Value<String> stokvelId;
  final Value<String> cycleId;
  final Value<int> amountInCents;
  final Value<int> createdAtUtc;
  final Value<int?> lastAttemptAtUtc;
  final Value<int> attemptCount;
  final Value<String> status;
  final Value<int> rowid;
  const PaymentOutboxCompanion({
    this.idempotencyKey = const Value.absent(),
    this.userId = const Value.absent(),
    this.stokvelId = const Value.absent(),
    this.cycleId = const Value.absent(),
    this.amountInCents = const Value.absent(),
    this.createdAtUtc = const Value.absent(),
    this.lastAttemptAtUtc = const Value.absent(),
    this.attemptCount = const Value.absent(),
    this.status = const Value.absent(),
    this.rowid = const Value.absent(),
  });
  PaymentOutboxCompanion.insert({
    required String idempotencyKey,
    required String userId,
    required String stokvelId,
    required String cycleId,
    required int amountInCents,
    required int createdAtUtc,
    this.lastAttemptAtUtc = const Value.absent(),
    this.attemptCount = const Value.absent(),
    this.status = const Value.absent(),
    this.rowid = const Value.absent(),
  }) : idempotencyKey = Value(idempotencyKey),
       userId = Value(userId),
       stokvelId = Value(stokvelId),
       cycleId = Value(cycleId),
       amountInCents = Value(amountInCents),
       createdAtUtc = Value(createdAtUtc);
  static Insertable<PaymentOutboxData> custom({
    Expression<String>? idempotencyKey,
    Expression<String>? userId,
    Expression<String>? stokvelId,
    Expression<String>? cycleId,
    Expression<int>? amountInCents,
    Expression<int>? createdAtUtc,
    Expression<int>? lastAttemptAtUtc,
    Expression<int>? attemptCount,
    Expression<String>? status,
    Expression<int>? rowid,
  }) {
    return RawValuesInsertable({
      if (idempotencyKey != null) 'idempotency_key': idempotencyKey,
      if (userId != null) 'user_id': userId,
      if (stokvelId != null) 'stokvel_id': stokvelId,
      if (cycleId != null) 'cycle_id': cycleId,
      if (amountInCents != null) 'amount_in_cents': amountInCents,
      if (createdAtUtc != null) 'created_at_utc': createdAtUtc,
      if (lastAttemptAtUtc != null) 'last_attempt_at_utc': lastAttemptAtUtc,
      if (attemptCount != null) 'attempt_count': attemptCount,
      if (status != null) 'status': status,
      if (rowid != null) 'rowid': rowid,
    });
  }

  PaymentOutboxCompanion copyWith({
    Value<String>? idempotencyKey,
    Value<String>? userId,
    Value<String>? stokvelId,
    Value<String>? cycleId,
    Value<int>? amountInCents,
    Value<int>? createdAtUtc,
    Value<int?>? lastAttemptAtUtc,
    Value<int>? attemptCount,
    Value<String>? status,
    Value<int>? rowid,
  }) {
    return PaymentOutboxCompanion(
      idempotencyKey: idempotencyKey ?? this.idempotencyKey,
      userId: userId ?? this.userId,
      stokvelId: stokvelId ?? this.stokvelId,
      cycleId: cycleId ?? this.cycleId,
      amountInCents: amountInCents ?? this.amountInCents,
      createdAtUtc: createdAtUtc ?? this.createdAtUtc,
      lastAttemptAtUtc: lastAttemptAtUtc ?? this.lastAttemptAtUtc,
      attemptCount: attemptCount ?? this.attemptCount,
      status: status ?? this.status,
      rowid: rowid ?? this.rowid,
    );
  }

  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    if (idempotencyKey.present) {
      map['idempotency_key'] = Variable<String>(idempotencyKey.value);
    }
    if (userId.present) {
      map['user_id'] = Variable<String>(userId.value);
    }
    if (stokvelId.present) {
      map['stokvel_id'] = Variable<String>(stokvelId.value);
    }
    if (cycleId.present) {
      map['cycle_id'] = Variable<String>(cycleId.value);
    }
    if (amountInCents.present) {
      map['amount_in_cents'] = Variable<int>(amountInCents.value);
    }
    if (createdAtUtc.present) {
      map['created_at_utc'] = Variable<int>(createdAtUtc.value);
    }
    if (lastAttemptAtUtc.present) {
      map['last_attempt_at_utc'] = Variable<int>(lastAttemptAtUtc.value);
    }
    if (attemptCount.present) {
      map['attempt_count'] = Variable<int>(attemptCount.value);
    }
    if (status.present) {
      map['status'] = Variable<String>(status.value);
    }
    if (rowid.present) {
      map['rowid'] = Variable<int>(rowid.value);
    }
    return map;
  }

  @override
  String toString() {
    return (StringBuffer('PaymentOutboxCompanion(')
          ..write('idempotencyKey: $idempotencyKey, ')
          ..write('userId: $userId, ')
          ..write('stokvelId: $stokvelId, ')
          ..write('cycleId: $cycleId, ')
          ..write('amountInCents: $amountInCents, ')
          ..write('createdAtUtc: $createdAtUtc, ')
          ..write('lastAttemptAtUtc: $lastAttemptAtUtc, ')
          ..write('attemptCount: $attemptCount, ')
          ..write('status: $status, ')
          ..write('rowid: $rowid')
          ..write(')'))
        .toString();
  }
}

abstract class _$AppDatabase extends GeneratedDatabase {
  _$AppDatabase(QueryExecutor e) : super(e);
  $AppDatabaseManager get managers => $AppDatabaseManager(this);
  late final $HomeCacheEntriesTable homeCacheEntries = $HomeCacheEntriesTable(
    this,
  );
  late final $StokvelDetailCacheEntriesTable stokvelDetailCacheEntries =
      $StokvelDetailCacheEntriesTable(this);
  late final $CycleCacheEntriesTable cycleCacheEntries =
      $CycleCacheEntriesTable(this);
  late final $PaymentOutboxTable paymentOutbox = $PaymentOutboxTable(this);
  @override
  Iterable<TableInfo<Table, Object?>> get allTables =>
      allSchemaEntities.whereType<TableInfo<Table, Object?>>();
  @override
  List<DatabaseSchemaEntity> get allSchemaEntities => [
    homeCacheEntries,
    stokvelDetailCacheEntries,
    cycleCacheEntries,
    paymentOutbox,
  ];
}

typedef $$HomeCacheEntriesTableCreateCompanionBuilder =
    HomeCacheEntriesCompanion Function({
      required String cacheKey,
      required String payload,
      required int cachedAtUtc,
      Value<int> rowid,
    });
typedef $$HomeCacheEntriesTableUpdateCompanionBuilder =
    HomeCacheEntriesCompanion Function({
      Value<String> cacheKey,
      Value<String> payload,
      Value<int> cachedAtUtc,
      Value<int> rowid,
    });

class $$HomeCacheEntriesTableFilterComposer
    extends Composer<_$AppDatabase, $HomeCacheEntriesTable> {
  $$HomeCacheEntriesTableFilterComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnFilters<String> get cacheKey => $composableBuilder(
    column: $table.cacheKey,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get payload => $composableBuilder(
    column: $table.payload,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<int> get cachedAtUtc => $composableBuilder(
    column: $table.cachedAtUtc,
    builder: (column) => ColumnFilters(column),
  );
}

class $$HomeCacheEntriesTableOrderingComposer
    extends Composer<_$AppDatabase, $HomeCacheEntriesTable> {
  $$HomeCacheEntriesTableOrderingComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnOrderings<String> get cacheKey => $composableBuilder(
    column: $table.cacheKey,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get payload => $composableBuilder(
    column: $table.payload,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<int> get cachedAtUtc => $composableBuilder(
    column: $table.cachedAtUtc,
    builder: (column) => ColumnOrderings(column),
  );
}

class $$HomeCacheEntriesTableAnnotationComposer
    extends Composer<_$AppDatabase, $HomeCacheEntriesTable> {
  $$HomeCacheEntriesTableAnnotationComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  GeneratedColumn<String> get cacheKey =>
      $composableBuilder(column: $table.cacheKey, builder: (column) => column);

  GeneratedColumn<String> get payload =>
      $composableBuilder(column: $table.payload, builder: (column) => column);

  GeneratedColumn<int> get cachedAtUtc => $composableBuilder(
    column: $table.cachedAtUtc,
    builder: (column) => column,
  );
}

class $$HomeCacheEntriesTableTableManager
    extends
        RootTableManager<
          _$AppDatabase,
          $HomeCacheEntriesTable,
          HomeCacheEntry,
          $$HomeCacheEntriesTableFilterComposer,
          $$HomeCacheEntriesTableOrderingComposer,
          $$HomeCacheEntriesTableAnnotationComposer,
          $$HomeCacheEntriesTableCreateCompanionBuilder,
          $$HomeCacheEntriesTableUpdateCompanionBuilder,
          (
            HomeCacheEntry,
            BaseReferences<
              _$AppDatabase,
              $HomeCacheEntriesTable,
              HomeCacheEntry
            >,
          ),
          HomeCacheEntry,
          PrefetchHooks Function()
        > {
  $$HomeCacheEntriesTableTableManager(
    _$AppDatabase db,
    $HomeCacheEntriesTable table,
  ) : super(
        TableManagerState(
          db: db,
          table: table,
          createFilteringComposer: () =>
              $$HomeCacheEntriesTableFilterComposer($db: db, $table: table),
          createOrderingComposer: () =>
              $$HomeCacheEntriesTableOrderingComposer($db: db, $table: table),
          createComputedFieldComposer: () =>
              $$HomeCacheEntriesTableAnnotationComposer($db: db, $table: table),
          updateCompanionCallback:
              ({
                Value<String> cacheKey = const Value.absent(),
                Value<String> payload = const Value.absent(),
                Value<int> cachedAtUtc = const Value.absent(),
                Value<int> rowid = const Value.absent(),
              }) => HomeCacheEntriesCompanion(
                cacheKey: cacheKey,
                payload: payload,
                cachedAtUtc: cachedAtUtc,
                rowid: rowid,
              ),
          createCompanionCallback:
              ({
                required String cacheKey,
                required String payload,
                required int cachedAtUtc,
                Value<int> rowid = const Value.absent(),
              }) => HomeCacheEntriesCompanion.insert(
                cacheKey: cacheKey,
                payload: payload,
                cachedAtUtc: cachedAtUtc,
                rowid: rowid,
              ),
          withReferenceMapper: (p0) => p0
              .map(
                (e) => (
                  e.readTable<$HomeCacheEntriesTable, HomeCacheEntry>(table),
                  BaseReferences<
                    _$AppDatabase,
                    $HomeCacheEntriesTable,
                    HomeCacheEntry
                  >(db, table, e),
                ),
              )
              .toList(),
          prefetchHooksCallback: null,
        ),
      );
}

typedef $$HomeCacheEntriesTableProcessedTableManager =
    ProcessedTableManager<
      _$AppDatabase,
      $HomeCacheEntriesTable,
      HomeCacheEntry,
      $$HomeCacheEntriesTableFilterComposer,
      $$HomeCacheEntriesTableOrderingComposer,
      $$HomeCacheEntriesTableAnnotationComposer,
      $$HomeCacheEntriesTableCreateCompanionBuilder,
      $$HomeCacheEntriesTableUpdateCompanionBuilder,
      (
        HomeCacheEntry,
        BaseReferences<_$AppDatabase, $HomeCacheEntriesTable, HomeCacheEntry>,
      ),
      HomeCacheEntry,
      PrefetchHooks Function()
    >;
typedef $$StokvelDetailCacheEntriesTableCreateCompanionBuilder =
    StokvelDetailCacheEntriesCompanion Function({
      required String cacheKey,
      required String payload,
      required int cachedAtUtc,
      Value<int> rowid,
    });
typedef $$StokvelDetailCacheEntriesTableUpdateCompanionBuilder =
    StokvelDetailCacheEntriesCompanion Function({
      Value<String> cacheKey,
      Value<String> payload,
      Value<int> cachedAtUtc,
      Value<int> rowid,
    });

class $$StokvelDetailCacheEntriesTableFilterComposer
    extends Composer<_$AppDatabase, $StokvelDetailCacheEntriesTable> {
  $$StokvelDetailCacheEntriesTableFilterComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnFilters<String> get cacheKey => $composableBuilder(
    column: $table.cacheKey,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get payload => $composableBuilder(
    column: $table.payload,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<int> get cachedAtUtc => $composableBuilder(
    column: $table.cachedAtUtc,
    builder: (column) => ColumnFilters(column),
  );
}

class $$StokvelDetailCacheEntriesTableOrderingComposer
    extends Composer<_$AppDatabase, $StokvelDetailCacheEntriesTable> {
  $$StokvelDetailCacheEntriesTableOrderingComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnOrderings<String> get cacheKey => $composableBuilder(
    column: $table.cacheKey,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get payload => $composableBuilder(
    column: $table.payload,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<int> get cachedAtUtc => $composableBuilder(
    column: $table.cachedAtUtc,
    builder: (column) => ColumnOrderings(column),
  );
}

class $$StokvelDetailCacheEntriesTableAnnotationComposer
    extends Composer<_$AppDatabase, $StokvelDetailCacheEntriesTable> {
  $$StokvelDetailCacheEntriesTableAnnotationComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  GeneratedColumn<String> get cacheKey =>
      $composableBuilder(column: $table.cacheKey, builder: (column) => column);

  GeneratedColumn<String> get payload =>
      $composableBuilder(column: $table.payload, builder: (column) => column);

  GeneratedColumn<int> get cachedAtUtc => $composableBuilder(
    column: $table.cachedAtUtc,
    builder: (column) => column,
  );
}

class $$StokvelDetailCacheEntriesTableTableManager
    extends
        RootTableManager<
          _$AppDatabase,
          $StokvelDetailCacheEntriesTable,
          StokvelDetailCacheEntry,
          $$StokvelDetailCacheEntriesTableFilterComposer,
          $$StokvelDetailCacheEntriesTableOrderingComposer,
          $$StokvelDetailCacheEntriesTableAnnotationComposer,
          $$StokvelDetailCacheEntriesTableCreateCompanionBuilder,
          $$StokvelDetailCacheEntriesTableUpdateCompanionBuilder,
          (
            StokvelDetailCacheEntry,
            BaseReferences<
              _$AppDatabase,
              $StokvelDetailCacheEntriesTable,
              StokvelDetailCacheEntry
            >,
          ),
          StokvelDetailCacheEntry,
          PrefetchHooks Function()
        > {
  $$StokvelDetailCacheEntriesTableTableManager(
    _$AppDatabase db,
    $StokvelDetailCacheEntriesTable table,
  ) : super(
        TableManagerState(
          db: db,
          table: table,
          createFilteringComposer: () =>
              $$StokvelDetailCacheEntriesTableFilterComposer(
                $db: db,
                $table: table,
              ),
          createOrderingComposer: () =>
              $$StokvelDetailCacheEntriesTableOrderingComposer(
                $db: db,
                $table: table,
              ),
          createComputedFieldComposer: () =>
              $$StokvelDetailCacheEntriesTableAnnotationComposer(
                $db: db,
                $table: table,
              ),
          updateCompanionCallback:
              ({
                Value<String> cacheKey = const Value.absent(),
                Value<String> payload = const Value.absent(),
                Value<int> cachedAtUtc = const Value.absent(),
                Value<int> rowid = const Value.absent(),
              }) => StokvelDetailCacheEntriesCompanion(
                cacheKey: cacheKey,
                payload: payload,
                cachedAtUtc: cachedAtUtc,
                rowid: rowid,
              ),
          createCompanionCallback:
              ({
                required String cacheKey,
                required String payload,
                required int cachedAtUtc,
                Value<int> rowid = const Value.absent(),
              }) => StokvelDetailCacheEntriesCompanion.insert(
                cacheKey: cacheKey,
                payload: payload,
                cachedAtUtc: cachedAtUtc,
                rowid: rowid,
              ),
          withReferenceMapper: (p0) => p0
              .map(
                (e) => (
                  e.readTable<
                    $StokvelDetailCacheEntriesTable,
                    StokvelDetailCacheEntry
                  >(table),
                  BaseReferences<
                    _$AppDatabase,
                    $StokvelDetailCacheEntriesTable,
                    StokvelDetailCacheEntry
                  >(db, table, e),
                ),
              )
              .toList(),
          prefetchHooksCallback: null,
        ),
      );
}

typedef $$StokvelDetailCacheEntriesTableProcessedTableManager =
    ProcessedTableManager<
      _$AppDatabase,
      $StokvelDetailCacheEntriesTable,
      StokvelDetailCacheEntry,
      $$StokvelDetailCacheEntriesTableFilterComposer,
      $$StokvelDetailCacheEntriesTableOrderingComposer,
      $$StokvelDetailCacheEntriesTableAnnotationComposer,
      $$StokvelDetailCacheEntriesTableCreateCompanionBuilder,
      $$StokvelDetailCacheEntriesTableUpdateCompanionBuilder,
      (
        StokvelDetailCacheEntry,
        BaseReferences<
          _$AppDatabase,
          $StokvelDetailCacheEntriesTable,
          StokvelDetailCacheEntry
        >,
      ),
      StokvelDetailCacheEntry,
      PrefetchHooks Function()
    >;
typedef $$CycleCacheEntriesTableCreateCompanionBuilder =
    CycleCacheEntriesCompanion Function({
      required String cacheKey,
      required String payload,
      required int cachedAtUtc,
      Value<int> rowid,
    });
typedef $$CycleCacheEntriesTableUpdateCompanionBuilder =
    CycleCacheEntriesCompanion Function({
      Value<String> cacheKey,
      Value<String> payload,
      Value<int> cachedAtUtc,
      Value<int> rowid,
    });

class $$CycleCacheEntriesTableFilterComposer
    extends Composer<_$AppDatabase, $CycleCacheEntriesTable> {
  $$CycleCacheEntriesTableFilterComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnFilters<String> get cacheKey => $composableBuilder(
    column: $table.cacheKey,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get payload => $composableBuilder(
    column: $table.payload,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<int> get cachedAtUtc => $composableBuilder(
    column: $table.cachedAtUtc,
    builder: (column) => ColumnFilters(column),
  );
}

class $$CycleCacheEntriesTableOrderingComposer
    extends Composer<_$AppDatabase, $CycleCacheEntriesTable> {
  $$CycleCacheEntriesTableOrderingComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnOrderings<String> get cacheKey => $composableBuilder(
    column: $table.cacheKey,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get payload => $composableBuilder(
    column: $table.payload,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<int> get cachedAtUtc => $composableBuilder(
    column: $table.cachedAtUtc,
    builder: (column) => ColumnOrderings(column),
  );
}

class $$CycleCacheEntriesTableAnnotationComposer
    extends Composer<_$AppDatabase, $CycleCacheEntriesTable> {
  $$CycleCacheEntriesTableAnnotationComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  GeneratedColumn<String> get cacheKey =>
      $composableBuilder(column: $table.cacheKey, builder: (column) => column);

  GeneratedColumn<String> get payload =>
      $composableBuilder(column: $table.payload, builder: (column) => column);

  GeneratedColumn<int> get cachedAtUtc => $composableBuilder(
    column: $table.cachedAtUtc,
    builder: (column) => column,
  );
}

class $$CycleCacheEntriesTableTableManager
    extends
        RootTableManager<
          _$AppDatabase,
          $CycleCacheEntriesTable,
          CycleCacheEntry,
          $$CycleCacheEntriesTableFilterComposer,
          $$CycleCacheEntriesTableOrderingComposer,
          $$CycleCacheEntriesTableAnnotationComposer,
          $$CycleCacheEntriesTableCreateCompanionBuilder,
          $$CycleCacheEntriesTableUpdateCompanionBuilder,
          (
            CycleCacheEntry,
            BaseReferences<
              _$AppDatabase,
              $CycleCacheEntriesTable,
              CycleCacheEntry
            >,
          ),
          CycleCacheEntry,
          PrefetchHooks Function()
        > {
  $$CycleCacheEntriesTableTableManager(
    _$AppDatabase db,
    $CycleCacheEntriesTable table,
  ) : super(
        TableManagerState(
          db: db,
          table: table,
          createFilteringComposer: () =>
              $$CycleCacheEntriesTableFilterComposer($db: db, $table: table),
          createOrderingComposer: () =>
              $$CycleCacheEntriesTableOrderingComposer($db: db, $table: table),
          createComputedFieldComposer: () =>
              $$CycleCacheEntriesTableAnnotationComposer(
                $db: db,
                $table: table,
              ),
          updateCompanionCallback:
              ({
                Value<String> cacheKey = const Value.absent(),
                Value<String> payload = const Value.absent(),
                Value<int> cachedAtUtc = const Value.absent(),
                Value<int> rowid = const Value.absent(),
              }) => CycleCacheEntriesCompanion(
                cacheKey: cacheKey,
                payload: payload,
                cachedAtUtc: cachedAtUtc,
                rowid: rowid,
              ),
          createCompanionCallback:
              ({
                required String cacheKey,
                required String payload,
                required int cachedAtUtc,
                Value<int> rowid = const Value.absent(),
              }) => CycleCacheEntriesCompanion.insert(
                cacheKey: cacheKey,
                payload: payload,
                cachedAtUtc: cachedAtUtc,
                rowid: rowid,
              ),
          withReferenceMapper: (p0) => p0
              .map(
                (e) => (
                  e.readTable<$CycleCacheEntriesTable, CycleCacheEntry>(table),
                  BaseReferences<
                    _$AppDatabase,
                    $CycleCacheEntriesTable,
                    CycleCacheEntry
                  >(db, table, e),
                ),
              )
              .toList(),
          prefetchHooksCallback: null,
        ),
      );
}

typedef $$CycleCacheEntriesTableProcessedTableManager =
    ProcessedTableManager<
      _$AppDatabase,
      $CycleCacheEntriesTable,
      CycleCacheEntry,
      $$CycleCacheEntriesTableFilterComposer,
      $$CycleCacheEntriesTableOrderingComposer,
      $$CycleCacheEntriesTableAnnotationComposer,
      $$CycleCacheEntriesTableCreateCompanionBuilder,
      $$CycleCacheEntriesTableUpdateCompanionBuilder,
      (
        CycleCacheEntry,
        BaseReferences<_$AppDatabase, $CycleCacheEntriesTable, CycleCacheEntry>,
      ),
      CycleCacheEntry,
      PrefetchHooks Function()
    >;
typedef $$PaymentOutboxTableCreateCompanionBuilder =
    PaymentOutboxCompanion Function({
      required String idempotencyKey,
      required String userId,
      required String stokvelId,
      required String cycleId,
      required int amountInCents,
      required int createdAtUtc,
      Value<int?> lastAttemptAtUtc,
      Value<int> attemptCount,
      Value<String> status,
      Value<int> rowid,
    });
typedef $$PaymentOutboxTableUpdateCompanionBuilder =
    PaymentOutboxCompanion Function({
      Value<String> idempotencyKey,
      Value<String> userId,
      Value<String> stokvelId,
      Value<String> cycleId,
      Value<int> amountInCents,
      Value<int> createdAtUtc,
      Value<int?> lastAttemptAtUtc,
      Value<int> attemptCount,
      Value<String> status,
      Value<int> rowid,
    });

class $$PaymentOutboxTableFilterComposer
    extends Composer<_$AppDatabase, $PaymentOutboxTable> {
  $$PaymentOutboxTableFilterComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnFilters<String> get idempotencyKey => $composableBuilder(
    column: $table.idempotencyKey,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get userId => $composableBuilder(
    column: $table.userId,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get stokvelId => $composableBuilder(
    column: $table.stokvelId,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get cycleId => $composableBuilder(
    column: $table.cycleId,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<int> get amountInCents => $composableBuilder(
    column: $table.amountInCents,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<int> get createdAtUtc => $composableBuilder(
    column: $table.createdAtUtc,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<int> get lastAttemptAtUtc => $composableBuilder(
    column: $table.lastAttemptAtUtc,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<int> get attemptCount => $composableBuilder(
    column: $table.attemptCount,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get status => $composableBuilder(
    column: $table.status,
    builder: (column) => ColumnFilters(column),
  );
}

class $$PaymentOutboxTableOrderingComposer
    extends Composer<_$AppDatabase, $PaymentOutboxTable> {
  $$PaymentOutboxTableOrderingComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnOrderings<String> get idempotencyKey => $composableBuilder(
    column: $table.idempotencyKey,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get userId => $composableBuilder(
    column: $table.userId,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get stokvelId => $composableBuilder(
    column: $table.stokvelId,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get cycleId => $composableBuilder(
    column: $table.cycleId,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<int> get amountInCents => $composableBuilder(
    column: $table.amountInCents,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<int> get createdAtUtc => $composableBuilder(
    column: $table.createdAtUtc,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<int> get lastAttemptAtUtc => $composableBuilder(
    column: $table.lastAttemptAtUtc,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<int> get attemptCount => $composableBuilder(
    column: $table.attemptCount,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get status => $composableBuilder(
    column: $table.status,
    builder: (column) => ColumnOrderings(column),
  );
}

class $$PaymentOutboxTableAnnotationComposer
    extends Composer<_$AppDatabase, $PaymentOutboxTable> {
  $$PaymentOutboxTableAnnotationComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  GeneratedColumn<String> get idempotencyKey => $composableBuilder(
    column: $table.idempotencyKey,
    builder: (column) => column,
  );

  GeneratedColumn<String> get userId =>
      $composableBuilder(column: $table.userId, builder: (column) => column);

  GeneratedColumn<String> get stokvelId =>
      $composableBuilder(column: $table.stokvelId, builder: (column) => column);

  GeneratedColumn<String> get cycleId =>
      $composableBuilder(column: $table.cycleId, builder: (column) => column);

  GeneratedColumn<int> get amountInCents => $composableBuilder(
    column: $table.amountInCents,
    builder: (column) => column,
  );

  GeneratedColumn<int> get createdAtUtc => $composableBuilder(
    column: $table.createdAtUtc,
    builder: (column) => column,
  );

  GeneratedColumn<int> get lastAttemptAtUtc => $composableBuilder(
    column: $table.lastAttemptAtUtc,
    builder: (column) => column,
  );

  GeneratedColumn<int> get attemptCount => $composableBuilder(
    column: $table.attemptCount,
    builder: (column) => column,
  );

  GeneratedColumn<String> get status =>
      $composableBuilder(column: $table.status, builder: (column) => column);
}

class $$PaymentOutboxTableTableManager
    extends
        RootTableManager<
          _$AppDatabase,
          $PaymentOutboxTable,
          PaymentOutboxData,
          $$PaymentOutboxTableFilterComposer,
          $$PaymentOutboxTableOrderingComposer,
          $$PaymentOutboxTableAnnotationComposer,
          $$PaymentOutboxTableCreateCompanionBuilder,
          $$PaymentOutboxTableUpdateCompanionBuilder,
          (
            PaymentOutboxData,
            BaseReferences<
              _$AppDatabase,
              $PaymentOutboxTable,
              PaymentOutboxData
            >,
          ),
          PaymentOutboxData,
          PrefetchHooks Function()
        > {
  $$PaymentOutboxTableTableManager(_$AppDatabase db, $PaymentOutboxTable table)
    : super(
        TableManagerState(
          db: db,
          table: table,
          createFilteringComposer: () =>
              $$PaymentOutboxTableFilterComposer($db: db, $table: table),
          createOrderingComposer: () =>
              $$PaymentOutboxTableOrderingComposer($db: db, $table: table),
          createComputedFieldComposer: () =>
              $$PaymentOutboxTableAnnotationComposer($db: db, $table: table),
          updateCompanionCallback:
              ({
                Value<String> idempotencyKey = const Value.absent(),
                Value<String> userId = const Value.absent(),
                Value<String> stokvelId = const Value.absent(),
                Value<String> cycleId = const Value.absent(),
                Value<int> amountInCents = const Value.absent(),
                Value<int> createdAtUtc = const Value.absent(),
                Value<int?> lastAttemptAtUtc = const Value.absent(),
                Value<int> attemptCount = const Value.absent(),
                Value<String> status = const Value.absent(),
                Value<int> rowid = const Value.absent(),
              }) => PaymentOutboxCompanion(
                idempotencyKey: idempotencyKey,
                userId: userId,
                stokvelId: stokvelId,
                cycleId: cycleId,
                amountInCents: amountInCents,
                createdAtUtc: createdAtUtc,
                lastAttemptAtUtc: lastAttemptAtUtc,
                attemptCount: attemptCount,
                status: status,
                rowid: rowid,
              ),
          createCompanionCallback:
              ({
                required String idempotencyKey,
                required String userId,
                required String stokvelId,
                required String cycleId,
                required int amountInCents,
                required int createdAtUtc,
                Value<int?> lastAttemptAtUtc = const Value.absent(),
                Value<int> attemptCount = const Value.absent(),
                Value<String> status = const Value.absent(),
                Value<int> rowid = const Value.absent(),
              }) => PaymentOutboxCompanion.insert(
                idempotencyKey: idempotencyKey,
                userId: userId,
                stokvelId: stokvelId,
                cycleId: cycleId,
                amountInCents: amountInCents,
                createdAtUtc: createdAtUtc,
                lastAttemptAtUtc: lastAttemptAtUtc,
                attemptCount: attemptCount,
                status: status,
                rowid: rowid,
              ),
          withReferenceMapper: (p0) => p0
              .map(
                (e) => (
                  e.readTable<$PaymentOutboxTable, PaymentOutboxData>(table),
                  BaseReferences<
                    _$AppDatabase,
                    $PaymentOutboxTable,
                    PaymentOutboxData
                  >(db, table, e),
                ),
              )
              .toList(),
          prefetchHooksCallback: null,
        ),
      );
}

typedef $$PaymentOutboxTableProcessedTableManager =
    ProcessedTableManager<
      _$AppDatabase,
      $PaymentOutboxTable,
      PaymentOutboxData,
      $$PaymentOutboxTableFilterComposer,
      $$PaymentOutboxTableOrderingComposer,
      $$PaymentOutboxTableAnnotationComposer,
      $$PaymentOutboxTableCreateCompanionBuilder,
      $$PaymentOutboxTableUpdateCompanionBuilder,
      (
        PaymentOutboxData,
        BaseReferences<_$AppDatabase, $PaymentOutboxTable, PaymentOutboxData>,
      ),
      PaymentOutboxData,
      PrefetchHooks Function()
    >;

class $AppDatabaseManager {
  final _$AppDatabase _db;
  $AppDatabaseManager(this._db);
  $$HomeCacheEntriesTableTableManager get homeCacheEntries =>
      $$HomeCacheEntriesTableTableManager(_db, _db.homeCacheEntries);
  $$StokvelDetailCacheEntriesTableTableManager get stokvelDetailCacheEntries =>
      $$StokvelDetailCacheEntriesTableTableManager(
        _db,
        _db.stokvelDetailCacheEntries,
      );
  $$CycleCacheEntriesTableTableManager get cycleCacheEntries =>
      $$CycleCacheEntriesTableTableManager(_db, _db.cycleCacheEntries);
  $$PaymentOutboxTableTableManager get paymentOutbox =>
      $$PaymentOutboxTableTableManager(_db, _db.paymentOutbox);
}
