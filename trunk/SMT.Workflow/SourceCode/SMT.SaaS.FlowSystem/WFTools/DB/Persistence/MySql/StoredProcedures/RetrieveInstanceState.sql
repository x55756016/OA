DELIMITER $$

DROP PROCEDURE IF EXISTS RetrieveInstanceState $$
CREATE PROCEDURE RetrieveInstanceState
(
  IN p_INSTANCE_ID CHAR(36)
  ,IN p_OWNER_ID CHAR(36)
  ,IN p_OWNED_UNTIL DATETIME
  ,OUT p_RESULT INTEGER
  ,OUT p_CURRENT_OWNER_ID CHAR(36)
)
BEGIN
  SET p_RESULT = 0;
  SET p_CURRENT_OWNER_ID = p_OWNER_ID;

  IF p_OWNER_ID IS NOT NULL THEN
    UPDATE INSTANCE_STATE
    SET
      OWNER_ID = p_OWNER_ID
      ,OWNED_UNTIL = p_OWNED_UNTIL
    WHERE
      INSTANCE_ID = p_INSTANCE_ID
      AND (
        OWNER_ID = p_OWNER_ID OR OWNER_ID IS NULL OR OWNED_UNTIL < UTC_TIMESTAMP()
      );

    IF ROW_COUNT() = 0 THEN
      /* Instance not found, or ownership conflict */
      SET p_CURRENT_OWNER_ID = NULL;
      SELECT
        OWNER_ID
      INTO
		p_CURRENT_OWNER_ID
      FROM
        INSTANCE_STATE
      WHERE
        INSTANCE_ID = p_INSTANCE_ID;

      IF p_CURRENT_OWNER_ID IS NOT NULL THEN
        /* Cannot retrieve the instance due to an ownership conflict */
        SET p_RESULT = -2;
      ELSE
        /* Cannot retrieve the instance because it doesn't exist */
        SET p_RESULT = -1;
      END IF;
    END IF;
  END IF;

  IF p_RESULT = 0 THEN
    SELECT
      STATE
    FROM
      INSTANCE_STATE
    WHERE
      INSTANCE_ID = p_INSTANCE_ID;

    IF ROW_COUNT() = 0 THEN
      /* Cannot retrieve the instance because it doesn't exist */
      SET p_RESULT = -1;
    END IF;
  END IF;
END $$

DELIMITER ;
