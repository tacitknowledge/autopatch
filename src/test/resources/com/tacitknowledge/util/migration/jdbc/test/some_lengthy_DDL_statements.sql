CREATE TABLE `ref_country` ( 
	`isoCode` varchar(10) NOT NULL DEFAULT '', 
	`perceivedCorruption` decimal(9,2) DEFAULT NULL, 
	`currency` char(3) NOT NULL DEFAULT 'USD', 
	PRIMARY KEY (`isoCode`) 
	) ENGINE=InnoDB DEFAULT CHARSET=latin1;

CREATE TABLE `ref_country_subdivision` ( 
	`isoCode` varchar(10) NOT NULL, 
	`countryCode` varchar(10) NOT NULL, 
	PRIMARY KEY (`isoCode`,`countryCode`) 
	) ENGINE=InnoDB DEFAULT CHARSET=utf8; 

CREATE TABLE `ref_state` ( 
	`isoCode` char(10) NOT NULL, 
	`countryCode` char(2) NOT NULL, 
	PRIMARY KEY (`isoCode`,`countryCode`) 
	) ENGINE=InnoDB DEFAULT CHARSET=utf8; 

CREATE TABLE `accounts` ( 
	`id` int(11) NOT NULL AUTO_INCREMENT, 
	`name` varchar(50) NOT NULL, 
	`createdBy` int(11) DEFAULT NULL, 
	`creationDate` datetime DEFAULT NULL, 
	`updatedBy` int(11) DEFAULT NULL, 
	`updateDate` datetime DEFAULT NULL, 
	`status` varchar(15) NOT NULL DEFAULT 'Pending', 
	`address` varchar(50) DEFAULT NULL, 
	`address2` varchar(50) DEFAULT NULL, 
	`address3` varchar(50) DEFAULT NULL, 
	`city` varchar(35) DEFAULT NULL, 
	`state` varchar(10) DEFAULT NULL, 
	`zip` varchar(15) DEFAULT NULL, 
	`country` varchar(25) DEFAULT NULL, 
	`phone` varchar(30) DEFAULT NULL, 
	`phone2` varchar(35) DEFAULT NULL, 
	`fax` varchar(30) DEFAULT NULL, 
	`contactID` mediumint(9) DEFAULT NULL, 
	`email` varchar(50) DEFAULT NULL, 
	`web_URL` varchar(50) DEFAULT NULL, 
	`nameIndex` varchar(50) DEFAULT NULL, 
	`reason` varchar(100) DEFAULT NULL, 
	`description` text, 
	`needsIndexing` tinyint(4) unsigned NOT NULL DEFAULT '1', 
	`parentID` int(11) DEFAULT NULL, 
	`currencyCode` char(3) DEFAULT 'USD', 
	`locale` varchar(5) DEFAULT 'en', 
	`timezone` varchar(50) DEFAULT NULL, 
	PRIMARY KEY (`id`), 
	KEY `name` (`name`), 
	KEY `nameIndex` (`nameIndex`) 
	) ENGINE=InnoDB AUTO_INCREMENT=33517 DEFAULT CHARSET=latin1; 

CREATE TABLE `provider_audit` ( 
	`id` int(11) NOT NULL AUTO_INCREMENT, 
	`creationDate` datetime DEFAULT NULL, 
	`createdBy` int(11) DEFAULT NULL, 
	`updateDate` datetime DEFAULT NULL, 
	`updatedBy` int(11) DEFAULT NULL, 
	`expiresDate` datetime DEFAULT NULL, 
	`effectiveDate` datetime DEFAULT NULL, 
	`assignedDate` datetime DEFAULT NULL, 
	`scheduledDate` datetime DEFAULT NULL, 
	`lastRecalculation` date DEFAULT NULL, 
	`score` tinyint(3) DEFAULT '0', 
	`contact` varchar(50) DEFAULT NULL, 
	`phone` varchar(25) DEFAULT NULL, 
	`phone2` varchar(255) DEFAULT NULL, 
	`address` varchar(50) DEFAULT NULL, 
	`address2` varchar(50) DEFAULT NULL, 
	`city` varchar(35) DEFAULT NULL, 
	`state` varchar(10) DEFAULT NULL, 
	`zip` varchar(10) DEFAULT NULL, 
	`country` varchar(50) DEFAULT NULL, 
	`latitude` float NOT NULL DEFAULT '0', 
	`longitude` float NOT NULL DEFAULT '0', 
	PRIMARY KEY (`id`) 
	) ENGINE=InnoDB AUTO_INCREMENT=761716 DEFAULT CHARSET=latin1 

CREATE TABLE `provider_info` ( 
	`id` int(11) NOT NULL, 
	`description` text, 
	`secondContact` varchar(50) DEFAULT NULL, 
	`secondPhone` varchar(50) DEFAULT NULL, 
	`secondEmail` varchar(50) DEFAULT NULL, 
	`billingContact` varchar(50) DEFAULT NULL, 
	`billingPhone` varchar(50) DEFAULT NULL, 
	`billingEmail` varchar(50) DEFAULT NULL, 
	`billingAddress` varchar(50) DEFAULT NULL, 
	`billingCity` varchar(35) DEFAULT NULL, 
	`billingState` varchar(10) DEFAULT NULL, 
	`billingCountrySubdivision` varchar(10) DEFAULT NULL, 
	`billingZip` varchar(10) DEFAULT NULL, 
	`billingCountry` varchar(25) DEFAULT NULL, 
	`membershipDate` date DEFAULT NULL, 
	`paymentMethod` varchar(20) DEFAULT 'CreditCard', 
	`paymentMethodStatus` varchar(20) DEFAULT NULL, 
	`renew` tinyint(4) DEFAULT '1', 
	`lastUpgradeDate` date DEFAULT NULL, 
	`balance` decimal(9,2) DEFAULT '0.00', 
	`needsRecalculation` tinyint(4) NOT NULL DEFAULT '1', 
	`lastRecalculation` datetime DEFAULT NULL, 
	`ccOnFile` tinyint(4) NOT NULL DEFAULT '0', 
	`ccExpiration` date DEFAULT NULL, 
	`ccEmail` varchar(50) DEFAULT NULL, 
	`showInDirectory` tinyint(4) DEFAULT '1', 
	PRIMARY KEY (`id`), 
	CONSTRAINT `FK_contractor_info` FOREIGN KEY (`id`) REFERENCES `accounts` (`id`) ON DELETE CASCADE 
	) ENGINE=InnoDB DEFAULT CHARSET=latin1 

CREATE TABLE `provider_registration_request` ( 
	`id` int(11) NOT NULL AUTO_INCREMENT, 
	`name` varchar(100) NOT NULL, 
	`createdBy` int(11) DEFAULT NULL, 
	`updatedBy` int(11) DEFAULT NULL, 
	`creationDate` datetime DEFAULT NULL, 
	`updateDate` datetime DEFAULT NULL, 
	`requestedByID` mediumint(9) NOT NULL, 
	`requestedByUser` varchar(20) DEFAULT NULL, 
	`status` varchar(30) NOT NULL DEFAULT 'Active', 
	`contact` varchar(30) NOT NULL, 
	`phone` varchar(20) DEFAULT NULL, 
	`email` varchar(50) DEFAULT NULL, 
	`address` varchar(100) DEFAULT NULL, 
	`city` varchar(50) DEFAULT NULL, 
	`state` varchar(10) DEFAULT NULL, 
	`zip` varchar(10) DEFAULT NULL, 
	`country` char(2) DEFAULT NULL, 
	`deadline` date DEFAULT NULL, 
	`lastContactedBy` mediumint(9) DEFAULT NULL, 
	`lastContactDate` datetime DEFAULT NULL, 
	`contactCountByEmail` tinyint(4) unsigned NOT NULL DEFAULT '0', 
	`contactCountByPhone` tinyint(4) unsigned NOT NULL DEFAULT '0', 
	`notes` text, 
	`holdDate` date DEFAULT NULL, 
	`reasonForRegistration` varchar(500) DEFAULT NULL, 
	`reasonForDecline` varchar(500) DEFAULT NULL, 
	`closedOnDate` date DEFAULT NULL, 
	PRIMARY KEY (`id`), 
	UNIQUE KEY `NameRequestedByIDUnique` (`name`,`requestedByID`), 
	KEY `status` (`status`,`country`,`state`) 
	) ENGINE=InnoDB AUTO_INCREMENT=7465 DEFAULT CHARSET=latin1 

	
	
ALTER TABLE `accounts` 
	ADD COLUMN `countrySubdivision` varchar(10) COLLATE latin1_swedish_ci NULL after `state`, 
	CHANGE `zip` `zip` varchar(15) COLLATE latin1_swedish_ci NULL after `countrySubdivision`, 
	CHANGE `country` `country` varchar(25) COLLATE latin1_swedish_ci NULL after `zip`, 
	CHANGE `phone` `phone` varchar(30) COLLATE latin1_swedish_ci NULL after `country`, 
	CHANGE `phone2` `phone2` varchar(35) COLLATE latin1_swedish_ci NULL after `phone`, 
	CHANGE `fax` `fax` varchar(30) COLLATE latin1_swedish_ci NULL after `phone2`, 
	CHANGE `contactID` `contactID` mediumint(9) NULL after `fax`, 
	CHANGE `email` `email` varchar(50) COLLATE latin1_swedish_ci NULL after `contactID`, 
	CHANGE `web_URL` `web_URL` varchar(50) COLLATE latin1_swedish_ci NULL after `email`, 
	CHANGE `nameIndex` `nameIndex` varchar(50) COLLATE latin1_swedish_ci NULL after `web_URL`, 
	CHANGE `reason` `reason` varchar(100) COLLATE latin1_swedish_ci NULL after `nameIndex`, 
	CHANGE `description` `description` text COLLATE latin1_swedish_ci NULL after `reason`, 
	CHANGE `needsIndexing` `needsIndexing` tinyint(4) unsigned NOT NULL DEFAULT '1' after `description`, 
	CHANGE `parentID` `parentID` int(11) NULL after `needsIndexing`, 
	CHANGE `currencyCode` `currencyCode` char(3) COLLATE latin1_swedish_ci NULL DEFAULT 'USD' after `parentID`, 
	CHANGE `locale` `locale` varchar(5) COLLATE latin1_swedish_ci NULL DEFAULT 'en' after `currencyCode`, 
	CHANGE `timezone` `timezone` varchar(50) COLLATE latin1_swedish_ci NULL after `locale`, COMMENT='';

ALTER TABLE `provider_audit` 
	CHANGE `state` `state` varchar(10) COLLATE latin1_swedish_ci NULL after `city`, 
	ADD COLUMN `countrySubdivision` varchar(10) COLLATE latin1_swedish_ci NULL after `state`, 
	CHANGE `zip` `zip` varchar(10) COLLATE latin1_swedish_ci NULL after `countrySubdivision`, 
	CHANGE `country` `country` varchar(50) COLLATE latin1_swedish_ci NULL after `zip`, 
	CHANGE `latitude` `latitude` float NOT NULL DEFAULT '0' after `country`, 
	CHANGE `longitude` `longitude` float NOT NULL DEFAULT '0' after `latitude`;

ALTER TABLE `provider_info` 
	ADD COLUMN `billingCountrySubdivision` varchar(10) COLLATE latin1_swedish_ci NULL after `billingState`;

ALTER TABLE `provider_registration_request` 
	ADD COLUMN `countrySubdivision` varchar(10) COLLATE latin1_swedish_ci NULL after `state`;

ALTER TABLE `provider_info`
	ADD CONSTRAINT `FK_provider_info` 
	FOREIGN KEY (`id`) REFERENCES `accounts` (`id`) ON DELETE CASCADE;

	
	
UPDATE accounts a
	JOIN ref_state rs on a.state = rs.isoCode and a.country = rs.countryCode
	SET countrySubdivision = concat(country,'-',state)
	WHERE state is not null 
	AND countrySubdivision is null;

UPDATE provider_audit a
	JOIN ref_state rs on a.state = rs.isoCode and a.country = rs.countryCode
	SET countrySubdivision = concat(country,'-',state)
	WHERE a.state is not null 
	AND a.countrySubdivision is null;

UPDATE provider_info a
	JOIN ref_state rs on a.billingState = rs.isoCode and a.billingCountry = rs.countryCode
	SET billingCountrySubdivision = concat(billingCountry,'-',billingState)
	WHERE a.billingState is not null 
	AND a.billingCountrySubdivision is null;

UPDATE provider_registration_request a
	JOIN ref_state rs on a.state = rs.isoCode and a.country = rs.countryCode
	SET countrySubdivision = concat(country,'-',state)
	WHERE state is not null 
	AND countrySubdivision is null;

