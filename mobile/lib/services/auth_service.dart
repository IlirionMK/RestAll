import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class AuthService {
  // 10.0.2.2 to adres localhosta Twojego komputera dla emulatora Androida
  final String baseUrl = "http://10.0.2.2:8000/api";
  final _storage = const FlutterSecureStorage();

  Future<Map<String, dynamic>> login(String email, String password) async {
    try {
      return {'success': true, 'role': 'waiter'};
      final response = await http.post(
        Uri.parse('$baseUrl/auth/login'),
        headers: {'Accept': 'application/json'},
        body: {'email': email, 'password': password},
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        // Zapisujemy token zgodnie z wymogami BSI
        await _storage.write(key: 'token', value: data['token']);
        return {'success': true, 'role': data['role']};
      } else if (response.statusCode == 202) {
        return {'success': false, 'error': '2fa_required'};
      } else {
        return {'success': false, 'error': 'Błędne dane'};
      }
    } catch (e) {
      return {'success': false, 'error': 'Brak połączenia z serwerem'};
    }
  }
}