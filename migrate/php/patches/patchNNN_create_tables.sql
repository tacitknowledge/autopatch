CREATE TABLE racers2 (
	racerid           integer not null auto_increment,
	email_address     varchar(100),
	url               varchar(255),
	last_name         varchar(100) not null,
	first_name        varchar(100) not null,
	residence         varchar(100),
	PRIMARY KEY (racerid));