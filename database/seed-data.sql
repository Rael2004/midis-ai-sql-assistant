USE MidisSqlAiDb;
GO

INSERT INTO Clients (ClientName, Country, Industry)
VALUES
('Cedar Bank', 'Lebanon', 'Banking'),
('Levant Retail Group', 'Lebanon', 'Retail'),
('Phoenicia Insurance', 'Lebanon', 'Insurance'),
('MedCare Hospital', 'Lebanon', 'Healthcare'),
('Beirut Logistics', 'Lebanon', 'Logistics');

INSERT INTO Departments (DepartmentName)
VALUES
('Infrastructure'),
('Networking'),
('Cloud Services'),
('Cybersecurity'),
('Application Support');

INSERT INTO Employees (FullName, Email, DepartmentId)
VALUES
('Karim Haddad', 'karim.haddad@example.com', 1),
('Maya Khoury', 'maya.khoury@example.com', 2),
('Nour Mansour', 'nour.mansour@example.com', 3),
('Rami Saade', 'rami.saade@example.com', 4),
('Lina Farah', 'lina.farah@example.com', 5);

INSERT INTO Services (ServiceName, Description)
VALUES
('Server Support', 'Support for physical and virtual servers'),
('Network Connectivity', 'Troubleshooting LAN, WAN, and internet issues'),
('Cloud Migration', 'Support for moving workloads to cloud platforms'),
('Security Monitoring', 'Monitoring and response for security alerts'),
('Business Application Support', 'Support for internal business applications');

INSERT INTO Tickets
(ClientId, ServiceId, AssignedEmployeeId, Title, Status, Priority, CreatedAt, ResolvedAt)
VALUES
(1, 1, 1, 'Database server performance issue', 'Resolved', 'High', '2026-07-01 09:15:00', '2026-07-01 15:30:00'),
(1, 4, 4, 'Suspicious login attempts detected', 'In Progress', 'Critical', '2026-07-03 10:00:00', NULL),
(2, 2, 2, 'Branch office internet instability', 'Open', 'High', '2026-07-04 08:45:00', NULL),
(2, 5, 5, 'POS application error during checkout', 'Resolved', 'Medium', '2026-07-02 11:20:00', '2026-07-02 14:10:00'),
(3, 4, 4, 'Firewall rule review request', 'Closed', 'Low', '2026-06-28 13:00:00', '2026-06-29 09:00:00'),
(3, 1, 1, 'Backup job failed overnight', 'Resolved', 'High', '2026-07-05 07:30:00', '2026-07-05 12:00:00'),
(4, 3, 3, 'Azure migration planning support', 'In Progress', 'Medium', '2026-07-06 09:00:00', NULL),
(4, 2, 2, 'Wi-Fi outage in administration building', 'Open', 'Critical', '2026-07-07 10:15:00', NULL),
(5, 1, 1, 'Storage capacity warning on file server', 'Open', 'Medium', '2026-07-08 14:25:00', NULL),
(5, 5, 5, 'ERP report generation error', 'Resolved', 'High', '2026-07-03 16:40:00', '2026-07-04 10:20:00'),
(1, 3, 3, 'Cloud cost optimization review', 'Closed', 'Low', '2026-06-25 12:00:00', '2026-06-26 12:30:00'),
(2, 4, 4, 'Endpoint protection alert', 'Resolved', 'Critical', '2026-07-09 09:10:00', '2026-07-09 11:00:00');

INSERT INTO TicketComments
(TicketId, EmployeeId, CommentText, CreatedAt)
VALUES
(1, 1, 'Checked SQL Server CPU and memory usage.', '2026-07-01 10:00:00'),
(1, 1, 'Optimized indexes and confirmed performance improvement.', '2026-07-01 15:20:00'),
(2, 4, 'Security logs are being reviewed.', '2026-07-03 11:00:00'),
(3, 2, 'ISP line quality test requested.', '2026-07-04 09:15:00'),
(4, 5, 'Application logs showed payment module timeout.', '2026-07-02 12:00:00'),
(6, 1, 'Backup service restarted successfully.', '2026-07-05 11:45:00'),
(7, 3, 'Initial cloud migration assessment started.', '2026-07-06 10:30:00'),
(8, 2, 'Access points are being checked.', '2026-07-07 11:00:00'),
(10, 5, 'ERP reporting service was restarted.', '2026-07-04 09:45:00'),
(12, 4, 'Endpoint isolated and alert resolved.', '2026-07-09 10:30:00');
GO