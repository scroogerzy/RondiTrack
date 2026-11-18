import 'package:json_annotation/json_annotation.dart';

extension type const Cents(int value) implements int {
  String formatEnZa() {
    final digits = (value.abs() ~/ 100).toString();
    final chunks = <String>[];
    for (var end = digits.length; end > 0; end -= 3) {
      chunks.insert(
        0,
        digits.substring((end - 3).clamp(0, digits.length), end),
      );
    }
    final whole = chunks.join('\u00a0');
    final fraction = (value.abs() % 100).toString().padLeft(2, '0');
    final sign = value < 0 ? '-' : '';
    return 'R\u00a0$sign$whole,$fraction';
  }

  static Cents? tryParse(String input) {
    final text = input
        .trim()
        .replaceFirst(RegExp(r'^[Rr]\s*'), '')
        .replaceAll('\u00a0', ' ');
    if (text.isEmpty ||
        text.endsWith('.') ||
        !RegExp(r'^\d[\d ]*(,\d{1,2})?$').hasMatch(text)) {
      return null;
    }
    final parts = text.split(',');
    final whole = int.tryParse(parts.first.replaceAll(' ', ''));
    if (whole == null) return null;
    final fraction = parts.length == 1
        ? 0
        : int.parse(parts[1].padRight(2, '0'));
    return Cents(whole * 100 + fraction);
  }
}

class CentsJsonConverter extends JsonConverter<Cents, int> {
  const new();

  @override
  Cents fromJson(int json) => Cents(json);

  @override
  int toJson(Cents object) => object.value;
}
