DELIMITER $$

DROP PROCEDURE IF EXISTS RetrieveNonblockingInstanceIds $$
CREATE PROCEDURE RetrieveNonblockingInstanceIds
(
  IN p_OWNER_ID CHAR(36)
  ,IN p_OWNED_UNTIL DATETIME
  ,IN p_NOW DATETIME
)
BEGIN

  /*
   *
   * Find instances that aren't blocked, completed,
   * terminated or suspended and aren't owned or
   * ownership has expired
   */
  SELECT
    INSTANCE_ID
  FROM
    INSTANCE_STATE
  WHERE
    BLOCKED = FALSE
    AND STATUS NOT IN (1, 2, 3)
    AND (OWNER_ID IS NULL OR OWNED_UNTIL < p_NOW)
  FOR UPDATE;

  IF ROW_COUNT() > 0 THEN
    UPDATE INSTANCE_STATE
    SET
      OWNER_ID = p_OWNER_ID
      ,OWNED_UNTIL = p_OWNED_UNTIL
    WHERE
      BLOCKED = FALSE
      AND STATUS NOT IN (1, 2, 3)
      AND (OWNER_ID IS NULL OR OWNED_UNTIL < p_NOW);
  END IF;
END $$

DELIMITER ;
