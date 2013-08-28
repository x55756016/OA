DELIMITER $$

DROP PROCEDURE IF EXISTS DeleteInstanceTrackingProfile $$
/*
 * Remove a tracking profile for a workflow instance.
 */
CREATE PROCEDURE DeleteInstanceTrackingProfile
(
	IN p_INSTANCE_ID CHAR(36)
)
BEGIN
	UPDATE TRACKING_PROFILE_INSTANCE
	SET
		TRACKING_PROFILE_XML = NULL
		,UPDATED_DATE_TIME = UTC_TIMESTAMP()
	WHERE
		INSTANCE_ID = p_INSTANCE_ID;

	
	IF ROW_COUNT() = 0 THEN
		-- no rows updated, perform an insert instead
		INSERT INTO TRACKING_PROFILE_INSTANCE
		(
			INSTANCE_ID
			,TRACKING_PROFILE_XML
			,UPDATED_DATE_TIME
		)
		VALUES
		(
			p_INSTANCE_ID
			,NULL
			,UTC_TIMESTAMP()
		);
	END IF;
END $$

DELIMITER ;
