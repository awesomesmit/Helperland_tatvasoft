create table city (
id int PRIMARY KEY IDENTITY,
name nvarchar(50) NOT NULL
);


create table user_type (
id int PRIMARY KEY IDENTITY,
type nvarchar(20) NOT NULL
);


create table user_address (
id int PRIMARY KEY IDENTITY,
street nvarchar(150) NOT NULL,
house nvarchar(20),
postal_code nvarchar(12) NOT NULL,
city_id int FOREIGN KEY REFERENCES city(id),
userid int,
doi datetime NOT NULL,
dou datetime,
isactive bit
);


create table user_details (
id int PRIMARY KEY IDENTITY,
first_name nvarchar(50) NOT NULL,
last_name nvarchar(50),
email nvarchar(70) UNIQUE,
mobile_no nvarchar(20),
gender nvarchar(10),
nationality nvarchar(20),
photo nvarchar(250),
date_of_birth date,
passwd nvarchar(70) NOT NULL,
user_type int FOREIGN KEY REFERENCES user_type(id),
address_id int FOREIGN KEY REFERENCES user_address(id),
user_status bit,
doi datetime NOT NULL,
dou datetime,
isactive bit
);


alter table user_address
add foreign key (userid) references user_details(id);

alter table user_address
add foreign key (userid) references user_details(id);

create table service_details(
id int primary key identity,
s_name nvarchar(30) NOT NULL,
photo nvarchar(250),
duration float NOT NULL,
amount float not null
);

create table service_req_status(
id int primary key identity,
sname nvarchar(30) NOT NULL
);

create table payment_status(
id int primary key identity,
sname nvarchar(30) NOT NULL
);

create table service_req(
id int primary key identity,
service_dt date NOT NULL,
service_req_id nvarchar(70) NOT NULL,
cust_id int foreign key references user_details(id) NOT NULL,
sp_id int foreign key references user_details(id),
service_addr_id int foreign key references user_address(id) NOT NULL,
billing_addr_id int foreign key references user_address(id),
pet_status bit NOT NULL,
discount float,
service_req_status int foreign key references service_req_status(id),
payment_status int foreign key references payment_status(id),
service_id int foreign key references service_details(id),
comments nvarchar(150)
);


create table avtar(
id int primary key identity,
photopath nvarchar(250)
);

create table sp_rating (
id int primary key identity,
cust_id int foreign key references user_details(id) NOT NULL,
sp_id int foreign key references user_details(id) NOT NULL,
on_time_arrival int NOT NULL,
friendly int NOT NULL,
quality_of_service int NOT NULL,
avg_rating float NOT NULL,
feedback nvarchar(100)
);


create table fav_sp (
id int primary key identity,
cust_id int foreign key references user_details(id) NOT NULL,
sp_id int foreign key references user_details(id) NOT NULL
);

create table blocked_sp (
id int primary key identity,
cust_id int foreign key references user_details(id) NOT NULL,
sp_id int foreign key references user_details(id) NOT NULL
);

create table blocked_cust (
id int primary key identity,
cust_id int foreign key references user_details(id) NOT NULL,
sp_id int foreign key references user_details(id) NOT NULL
);

create table contact_us(
id int primary key identity,
first_name nvarchar(50) NOT NULL,
last_name nvarchar(50),
email nvarchar(70) NOT NULL,
mobile_no nvarchar(12),
sub nvarchar(70),
msg nvarchar(300),
file_path nvarchar(250)
);


create table notification_tbl(
id int primary key identity,
sub nvarchar(70),
msg nvarchar(300),
userid int foreign key references user_details(id),
doi datetime not null,
dou datetime
);