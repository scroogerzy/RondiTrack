import 'package:flutter_test/flutter_test.dart';
import 'package:ronditrack_mobile/core/money/cents.dart';

void main() {
  test('parses accepted South African rand inputs', () {
    expect(Cents.tryParse('150')?.value, 15000);
    expect(Cents.tryParse('150,50')?.value, 15050);
    expect(Cents.tryParse('R150')?.value, 15000);
    expect(Cents.tryParse('R 1 500')?.value, 150000);
    expect(Cents.tryParse('1 500,50')?.value, 150050);
    expect(Cents.tryParse('1\u00a0500,50')?.value, 150050);
  });
  test('rejects ambiguous or invalid input', () {
    for (final value in [
      '1,500.50',
      '1,2,3',
      '10,555',
      '10.',
      '',
      'letters',
      'R',
      '-10',
    ]) {
      expect(Cents.tryParse(value), isNull, reason: value);
    }
  });
  test('formats cents using NBSP grouping', () {
    expect(const Cents(150050).formatEnZa(), 'R\u00a01\u00a0500,50');
    expect(const Cents(15000).formatEnZa(), 'R\u00a0150,00');
  });
}
