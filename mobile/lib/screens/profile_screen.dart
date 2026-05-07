import 'package:flutter/material.dart';
import '../services/api_service.dart';

class ProfileScreen extends StatefulWidget {
  const ProfileScreen({super.key});
  @override
  State<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends State<ProfileScreen> {
  final _apiService = ApiService();
  final _nameController = TextEditingController();
  final _emailController = TextEditingController();
  bool _isLoading = false;

  @override
  void initState() {
    super.initState();
    // Pobieramy dane użytkownika przekazane podczas logowania
    _nameController.text = ApiService.currentUser?['name'] ?? "";
    _emailController.text = ApiService.currentUser?['email'] ?? "";
  }

  void _update() async {
    if (_nameController.text.isEmpty || _emailController.text.isEmpty) return;
    setState(() => _isLoading = true);
    bool ok = await _apiService.updateProfile(_nameController.text, _emailController.text);
    setState(() => _isLoading = false);
    
    if (mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(ok ? "Profil zaktualizowany! (RES-21)" : "Błąd aktualizacji profilu"), 
          backgroundColor: ok ? Colors.green : Colors.redAccent
        )
      );
    }
  }

  void _showChangePasswordDialog() {
    final currentPass = TextEditingController();
    final newPass = TextEditingController();
    
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text("Zmień hasło"),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            TextField(controller: currentPass, obscureText: true, decoration: const InputDecoration(labelText: "Obecne hasło")),
            const SizedBox(height: 10),
            TextField(controller: newPass, obscureText: true, decoration: const InputDecoration(labelText: "Nowe hasło")),
          ],
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context), child: const Text("ANULUJ")),
          ElevatedButton(
            onPressed: () async {
              if (currentPass.text.isEmpty || newPass.text.isEmpty) return;
              Navigator.pop(context);
              bool ok = await _apiService.changePassword(currentPass.text, newPass.text);
              if (mounted) {
                ScaffoldMessenger.of(context).showSnackBar(
                  SnackBar(content: Text(ok ? "Hasło zmienione pomyślnie!" : "Błąd podczas zmiany hasła"))
                );
              }
            }, 
            child: const Text("ZMIEŃ")
          )
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.white,
      appBar: AppBar(
        title: const Text("Twój Profil", style: TextStyle(fontWeight: FontWeight.bold)),
        backgroundColor: Colors.deepOrange,
        foregroundColor: Colors.white,
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(30),
        child: Column(
          children: [
            const CircleAvatar(
              radius: 60,
              backgroundColor: Colors.deepOrange,
              child: Icon(Icons.person, size: 70, color: Colors.white),
            ),
            const SizedBox(height: 15),
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 6),
              decoration: BoxDecoration(color: Colors.orange[50], borderRadius: BorderRadius.circular(20)),
              child: Text(
                (ApiService.userRole ?? "Gość").toUpperCase(), 
                style: const TextStyle(fontWeight: FontWeight.bold, color: Colors.deepOrange, fontSize: 12)
              ),
            ),
            const SizedBox(height: 40),
            TextField(
              controller: _nameController,
              decoration: const InputDecoration(labelText: "Imię i Nazwisko", prefixIcon: Icon(Icons.person_outline)),
            ),
            const SizedBox(height: 20),
            TextField(
              controller: _emailController,
              decoration: const InputDecoration(labelText: "Adres Email", prefixIcon: Icon(Icons.email_outlined)),
            ),
            const SizedBox(height: 40),
            _isLoading 
              ? const CircularProgressIndicator()
              : ElevatedButton(
                  onPressed: _update, 
                  child: const Text("ZAKTUALIZUJ DANE"),
                ),
            const SizedBox(height: 15),
            OutlinedButton.icon(
              style: OutlinedButton.styleFrom(minimumSize: const Size.fromHeight(55)),
              icon: const Icon(Icons.lock_reset),
              label: const Text("ZMIEŃ HASŁO"),
              onPressed: _showChangePasswordDialog,
            ),
          ],
        ),
      ),
    );
  }
}
