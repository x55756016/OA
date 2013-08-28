DELIMITER $$

DROP PROCEDURE IF EXISTS RetrieveExpiredTimerIds $$
CREATE PROCEDURE RetrieveExpiredTimerIds
(
  IN p_OWNER_ID CHAR(36)
  ,IN p_OWNED_UNTIL DATETIME
  ,IN p_NOW DATETIME
)
BEGIN
  /*
   *
   * Find instances that don't have a status of completed,
   * terminated or suspended, have an expired timer, and
   * that are locked with no owner or their ownership has
   * expired.
   */
  SELECT
    INSTANCE_ID
  FROM
    INSTANCE_STATE
  WHERE
    NEXT_TIMER < p_NOW
    AND STATUS NOT IN (1, 2, 3)
    AND (
      (UNLOCKED = TRUE AND OWNER_ID IS NULL)
      OR (OWNED_UNTIL < p_NOW)
    );
END $$

DELIMITER ;
