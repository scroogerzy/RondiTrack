import 'package:drift/drift.dart';
import 'package:drift/native.dart';
import 'package:drift_flutter/drift_flutter.dart';

part 'app_database.g.dart';

class HomeCacheEntries extends Table {
  TextColumn get cacheKey => text()();
  TextColumn get payload => text()();
  IntColumn get cachedAtUtc => integer()();

  @override
  Set<Column> get primaryKey => {cacheKey};
}

class StokvelDetailCacheEntries extends Table {
  TextColumn get cacheKey => text()();
  TextColumn get payload => text()();
  IntColumn get cachedAtUtc => integer()();

  @override
  Set<Column> get primaryKey => {cacheKey};
}

class CycleCacheEntries extends Table {
  TextColumn get cacheKey => text()();
  TextColumn get payload => text()();
  IntColumn get cachedAtUtc => integer()();

  @override
  Set<Column> get primaryKey => {cacheKey};
}

class PaymentOutbox extends Table {
  TextColumn get idempotencyKey => text()();
  TextColumn get userId => text()();
  TextColumn get stokvelId => text()();
  TextColumn get cycleId => text()();
  IntColumn get amountInCents => integer()();
  IntColumn get createdAtUtc => integer()();
  IntColumn get lastAttemptAtUtc => integer().nullable()();
  IntColumn get attemptCount => integer().withDefault(const Constant(0))();
  TextColumn get status => text().withDefault(const Constant('pending'))();

  @override
  Set<Column> get primaryKey => {idempotencyKey};
}

@DriftDatabase(
  tables: [
    HomeCacheEntries,
    StokvelDetailCacheEntries,
    CycleCacheEntries,
    PaymentOutbox,
  ],
)
class AppDatabase extends _$AppDatabase {
  AppDatabase() : super(driftDatabase(name: 'ronditrack'));

  AppDatabase.forTesting() : super(NativeDatabase.memory());

  @override
  int get schemaVersion => 1;

  Future<void> enqueuePayment({
    required String idempotencyKey,
    required String userId,
    required String stokvelId,
    required String cycleId,
    required int amountInCents,
    required DateTime createdAtUtc,
  }) async {
    if (amountInCents <= 0) {
      throw ArgumentError.value(
        amountInCents,
        'amountInCents',
        'Payment amount must be positive.',
      );
    }

    final existing =
        await (select(paymentOutbox)..where(
              (row) =>
                  row.userId.equals(userId) &
                  row.stokvelId.equals(stokvelId) &
                  row.cycleId.equals(cycleId),
            ))
            .getSingleOrNull();

    if (existing != null) {
      throw StateError(
        'A payment for this member and cycle is already pending.',
      );
    }

    await into(paymentOutbox).insert(
      PaymentOutboxCompanion.insert(
        idempotencyKey: idempotencyKey,
        userId: userId,
        stokvelId: stokvelId,
        cycleId: cycleId,
        amountInCents: amountInCents,
        createdAtUtc: createdAtUtc.toUtc().millisecondsSinceEpoch,
      ),
    );
  }

  Future<List<PaymentOutboxData>> getPendingPayments() {
    return (select(paymentOutbox)
          ..where((row) => row.status.equals('pending'))
          ..orderBy([(row) => OrderingTerm.asc(row.createdAtUtc)]))
        .get();
  }

  Future<void> recordPaymentAttempt(String idempotencyKey) async {
    final row =
        await (select(paymentOutbox)
              ..where((item) => item.idempotencyKey.equals(idempotencyKey)))
            .getSingleOrNull();

    if (row == null) return;

    await (update(
      paymentOutbox,
    )..where((item) => item.idempotencyKey.equals(idempotencyKey))).write(
      PaymentOutboxCompanion(
        lastAttemptAtUtc: Value(DateTime.now().toUtc().millisecondsSinceEpoch),
        attemptCount: Value(row.attemptCount + 1),
      ),
    );
  }

  Future<void> removePayment(String idempotencyKey) {
    return (delete(
      paymentOutbox,
    )..where((row) => row.idempotencyKey.equals(idempotencyKey))).go();
  }

  Future<void> clearUserData() async {
    await transaction(() async {
      await delete(paymentOutbox).go();
      await delete(homeCacheEntries).go();
      await delete(stokvelDetailCacheEntries).go();
      await delete(cycleCacheEntries).go();
    });
  }

  Future<void> saveHomeCache({
    required String key,
    required String payload,
    required DateTime cachedAtUtc,
  }) {
    return into(homeCacheEntries).insertOnConflictUpdate(
      HomeCacheEntriesCompanion.insert(
        cacheKey: key,
        payload: payload,
        cachedAtUtc: cachedAtUtc.toUtc().millisecondsSinceEpoch,
      ),
    );
  }

  Future<void> saveStokvelDetailCache({
    required String key,
    required String payload,
    required DateTime cachedAtUtc,
  }) {
    return into(stokvelDetailCacheEntries).insertOnConflictUpdate(
      StokvelDetailCacheEntriesCompanion.insert(
        cacheKey: key,
        payload: payload,
        cachedAtUtc: cachedAtUtc.toUtc().millisecondsSinceEpoch,
      ),
    );
  }

  Future<void> saveCycleCache({
    required String key,
    required String payload,
    required DateTime cachedAtUtc,
  }) {
    return into(cycleCacheEntries).insertOnConflictUpdate(
      CycleCacheEntriesCompanion.insert(
        cacheKey: key,
        payload: payload,
        cachedAtUtc: cachedAtUtc.toUtc().millisecondsSinceEpoch,
      ),
    );
  }
}
