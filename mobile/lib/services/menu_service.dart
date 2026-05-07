import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class MenuService {
  final String baseUrl = "http://10.0.2.2:8000/api";
  final _storage = const FlutterSecureStorage();

  Future<List<dynamic>> getMenuItems() async {
    final token = await _storage.read(key: 'token');

    try {
      final response = await http.get(
        Uri.parse('$baseUrl/menu'),
        headers: {
          'Accept': 'application/json',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        return jsonDecode(response.body);
      } else {
        throw Exception('Błąd pobierania menu');
      }
    } catch (e) {
      // Jeśli serwer nie działa, zwracamy puste menu lub błąd
      return [];
    }
  }
}