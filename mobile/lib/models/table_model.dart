class TableModel {
  final int id;
  final String status;
  final int number;

  TableModel({required this.id, required this.status, required this.number});

  factory TableModel.fromJson(Map<String, dynamic> json) {
    return TableModel(
      id: json['id'],
      status: json['status'], // free, occupied, reserved
      number: json['number'],
    );
  }
}