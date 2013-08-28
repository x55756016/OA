DELIMITER $$

DROP PROCEDURE IF EXISTS InsertInstanceState $$
CREATE PROCEDURE InsertInstanceState
(
  IN p_INSTANCE_ID CHAR(36)
  ,IN p_STATE BLOB
  ,IN p_STATUS INTEGER
  ,IN p_UNLOCKED BOOLEAN
  ,IN p_BLOCKED BOOLEAN
  ,IN p_INFO TEXT
  ,IN p_OWNER_ID CHAR(36)
  ,IN p_OWNED_UNTIL DATETIME
  ,IN p_NEXT_TIMER DATETIME
  ,OUT p_RESULT INTEGER
  ,OUT p_CURRENT_OWNER_ID CHAR(36)
)
BEGIN
  DECLARE l_NOW DATETIME DEFAULT UTC_TIMESTAMP();

  SET p_RESULT = 0;
  SET p_CURRENT_OWNER_ID = p_OWNER_ID;

  IF p_STATUS = 1 OR p_STATUS = 3 THEN
    /* Status is completed or terminated */
    /* Remove all instance_state records and all completed_scope records */
    DELETE FROM INSTANCE_STATE
    WHERE
      INSTANCE_ID = p_INSTANCE_ID
      AND (
        (OWNER_ID = p_OWNER_ID AND OWNED_UNTIL >= l_NOW)
        OR (OWNER_ID IS NULL AND OWNED_UNTIL IS NULL)
      );

    IF ROW_COUNT() = 0 THEN
      SET p_CURRENT_OWNER_ID = NULL;
      SELECT
        p_CURRENT_OWNER_ID = OWNER_ID
      FROM
        INSTANCE_STATE
      WHERE
        INSTANCE_ID = p_INSTANCE_ID;

      IF p_CURRENT_OWNER_ID IS NOT NULL THEN
        /* Cannot remove the instance due to an ownership conflict */
        SET p_RESULT = 2;
      END IF;
    ELSE
      DELETE FROM COMPLETED_SCOPE
      WHERE
        INSTANCE_ID = p_INSTANCE_ID;
    END IF;
  ELSE
    IF NOT EXISTS (SELECT 1 FROM INSTANCE_STATE WHERE INSTANCE_ID = p_INSTANCE_ID) THEN
      /* Create a new record for this instance */
      IF p_UNLOCKED = FALSE THEN
        INSERT INTO INSTANCE_STATE
        (
          INSTANCE_ID ,STATE ,STATUS
          ,UNLOCKED ,BLOCKED ,INFO ,MODIFIED
          ,OWNER_ID ,OWNED_UNTIL ,NEXT_TIMER
        )
        VALUES
        (
          p_INSTANCE_ID ,p_STATE ,p_STATUS
          ,p_UNLOCKED ,p_BLOCKED ,p_INFO ,l_NOW
          ,p_OWNER_ID ,p_OWNED_UNTIL ,p_NEXT_TIMER
        );
      ELSE
        INSERT INTO INSTANCE_STATE
        (
          INSTANCE_ID ,STATE ,STATUS
          ,UNLOCKED ,BLOCKED ,INFO ,MODIFIED
          ,OWNER_ID ,OWNED_UNTIL ,NEXT_TIMER
        )
        VALUES
        (
          p_INSTANCE_ID ,p_STATE ,p_STATUS
          ,p_UNLOCKED ,p_BLOCKED ,p_INFO ,l_NOW
          ,NULL ,NULL ,p_NEXT_TIMER
        );
      END IF;
    ELSE
      /* Updating an existing record for this instance */
      IF p_UNLOCKED = FALSE THEN
        UPDATE INSTANCE_STATE
        SET
          STATE = p_STATE
          ,STATUS = p_STATUS
          ,UNLOCKED = p_UNLOCKED
          ,BLOCKED = p_BLOCKED
          ,INFO = p_INFO
          ,MODIFIED = l_NOW
          ,OWNER_ID = p_OWNER_ID
          ,OWNED_UNTIL = p_OWNED_UNTIL
          ,NEXT_TIMER = p_NEXT_TIMER
        WHERE
          INSTANCE_ID = p_INSTANCE_ID;
      ELSE
        UPDATE INSTANCE_STATE
        SET
          STATE = p_STATE
          ,STATUS = p_STATUS
          ,UNLOCKED = p_UNLOCKED
          ,BLOCKED = p_BLOCKED
          ,INFO = p_INFO
          ,MODIFIED = l_NOW
          ,OWNER_ID = NULL
          ,OWNED_UNTIL = NULL
          ,NEXT_TIMER = p_NEXT_TIMER
        WHERE
          INSTANCE_ID = p_INSTANCE_ID;
      END IF;
    END IF;
  END IF;
END $$

DELIMITER ;
