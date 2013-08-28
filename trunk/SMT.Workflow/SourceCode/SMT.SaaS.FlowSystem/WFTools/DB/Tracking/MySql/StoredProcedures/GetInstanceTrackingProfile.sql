DELIMITER $$

DROP PROCEDURE IF EXISTS GetInstanceTrackingProfile $$
/*
 * Retrieve the tracking profile for a workflow instance.
 */
CREATE PROCEDURE GetInstanceTrackingProfile
(
	IN p_INSTANCE_ID CHAR(36)
)
BEGIN
	SELECT
		TRACKING_PROFILE_XML
	FROM
		TRACKING_PROFILE_INSTANCE
	WHERE
		INSTANCE_ID = p_INSTANCE_ID;
	
END $$

DELIMITER ;
