DELIMITER $$

DROP PROCEDURE IF EXISTS GetDefaultTrackingProfile $$
/*
 * Retrieve a particular version of the default tracking profile.
 */	
CREATE PROCEDURE GetDefaultTrackingProfile
(
	IN p_VERSION VARCHAR(32)
)
BEGIN
	SELECT
		TRACKING_PROFILE_XML
	FROM
		DEFAULT_TRACKING_PROFILE
	WHERE
		VERSION = p_VERSION;
	
END $$

DELIMITER ;
