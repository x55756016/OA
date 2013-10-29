DELIMITER $$

DROP PROCEDURE IF EXISTS GetTypeId $$
/*
 * Retrieve the unique identifier of a type, creating a new one
 * if necessary.
 */	 
CREATE PROCEDURE GetTypeId
(
	OUT p_TYPE_ID BIGINT UNSIGNED
	,IN p_TYPE_FULL_NAME VARCHAR(128)
	,IN p_ASSEMBLY_FULL_NAME VARCHAR(256)
	,IN p_IS_INSTANCE_TYPE BOOLEAN
)
BEGIN
	IF p_IS_INSTANCE_TYPE IS NULL THEN
		SET p_IS_INSTANCE_TYPE = FALSE;
	END IF;
	
	SELECT
		TYPE_ID
	INTO
		p_TYPE_ID
	FROM
		TYPE
	WHERE
		TYPE_FULL_NAME = p_TYPE_FULL_NAME
		AND ASSEMBLY_FULL_NAME = p_ASSEMBLY_FULL_NAME;

	IF p_TYPE_ID IS NULL THEN
		INSERT INTO TYPE
		(
			TYPE_FULL_NAME
			,ASSEMBLY_FULL_NAME
			,IS_INSTANCE_TYPE
		)
		VALUES
		(
			p_TYPE_FULL_NAME
			,p_ASSEMBLY_FULL_NAME
			,p_IS_INSTANCE_TYPE
		);
		
		SET p_TYPE_ID = LAST_INSERT_ID();
  END IF;
END $$

DELIMITER ;