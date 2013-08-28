USE [WFTOOLS]
GO
/****** Object:  StoredProcedure [dbo].[InsertActivityExecutionStatusEventMultiple]    Script Date: 10/17/2007 00:39:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[InsertActivityExecutionStatusEventMultiple]	@WorkflowInstanceId					uniqueidentifier	= NULL
													,@WorkflowInstanceInternalId		bigint				= NULL OUTPUT /* IN/OUT */
													,@WorkflowInstanceContextGuid		uniqueidentifier	= NULL
													,@ActivityInstanceId1				bigint				= NULL OUTPUT /* IN/OUT */
													,@QualifiedName1					nvarchar(128)
													,@ContextGuid1						uniqueidentifier
													,@ParentContextGuid1				uniqueidentifier
													,@ExecutionStatusId1				tinyint		
													,@EventDateTime1					datetime	
													,@EventOrder1						int
													,@ActivityExecutionStatusEventId1	bigint OUTPUT
													,@ActivityInstanceId2				bigint				= NULL OUTPUT /* IN/OUT */
													,@QualifiedName2					nvarchar(128)		= NULL
													,@ContextGuid2						uniqueidentifier	= NULL
													,@ParentContextGuid2				uniqueidentifier	= NULL
													,@ExecutionStatusId2				tinyint				= NULL
													,@EventDateTime2					datetime			= NULL
													,@EventOrder2						int					= NULL
													,@ActivityExecutionStatusEventId2	bigint				= NULL OUTPUT		
													,@ActivityInstanceId3				bigint				= NULL OUTPUT /* IN/OUT */
													,@QualifiedName3					nvarchar(128)		= NULL
													,@ContextGuid3						uniqueidentifier	= NULL
													,@ParentContextGuid3				uniqueidentifier	= NULL
													,@ExecutionStatusId3				tinyint				= NULL
													,@EventDateTime3					datetime			= NULL
													,@EventOrder3						int					= NULL
													,@ActivityExecutionStatusEventId3	bigint				= NULL OUTPUT
													,@ActivityInstanceId4				bigint				= NULL OUTPUT /* IN/OUT */
													,@QualifiedName4					nvarchar(128)		= NULL
													,@ContextGuid4						uniqueidentifier	= NULL
													,@ParentContextGuid4				uniqueidentifier	= NULL
													,@ExecutionStatusId4				tinyint				= NULL
													,@EventDateTime4					datetime			= NULL
													,@EventOrder4						int					= NULL
													,@ActivityExecutionStatusEventId4	bigint				= NULL OUTPUT
													,@ActivityInstanceId5				bigint				= NULL OUTPUT /* IN/OUT */
													,@QualifiedName5					nvarchar(128)		= NULL
													,@ContextGuid5						uniqueidentifier	= NULL
													,@ParentContextGuid5				uniqueidentifier	= NULL
													,@ExecutionStatusId5				tinyint				= NULL
													,@EventDateTime5					datetime			= NULL
													,@EventOrder5						int					= NULL
													,@ActivityExecutionStatusEventId5	bigint				= NULL OUTPUT
AS
 BEGIN
	SET NOCOUNT ON	

	SET TRANSACTION ISOLATION LEVEL READ COMMITTED

	DECLARE @local_tran		bit
			,@error			int
			,@error_desc	nvarchar(256)
			,@ret			smallint

	declare @localized_string_InsertActivityExecutionStatusEvent_Failed_GetType nvarchar(256)
	set @localized_string_InsertActivityExecutionStatusEvent_Failed_GetType = N'InsertActivityExecutionStatusEvent failed calling procedure GetTypeId'

	declare @localized_string_InsertActivityExecutionStatusEvent_Failed_InvalidParam nvarchar(256)
	set @localized_string_InsertActivityExecutionStatusEvent_Failed_InvalidParam = N'@WorkflowInstanceId and @WorkflowInstanceInternalId cannot both be null'

	declare @localized_string_InsertActivityExecutionStatusEvent_Failed_WorkflowInstanceInternalId nvarchar(256)
	set @localized_string_InsertActivityExecutionStatusEvent_Failed_WorkflowInstanceInternalId = N'Failed calling GetWorkflowInstanceInternalId'

	declare @localized_string_InsertActivityExecutionStatusEvent_Failed_ActivityStatusInsert nvarchar(256)
	set @localized_string_InsertActivityExecutionStatusEvent_Failed_ActivityStatusInsert = N'Failed inserting into ActivityExecutionStatusEvent'

	declare @localized_string_InsertActivityExecutionStatusEvent_Failed_ActivityInstanceIdSel nvarchar(256)
	set @localized_string_InsertActivityExecutionStatusEvent_Failed_ActivityInstanceIdSel = N'Failed calling GetActivityInstanceId'

	declare @localized_string_InsertActivityExecutionStatusEvent_Failed_NoEventId nvarchar(256)
	set @localized_string_InsertActivityExecutionStatusEvent_Failed_NoEventId = N'@ActivityExecutionStatusEventId is null or less than 0'


	IF @@TRANCOUNT > 0
		SET @local_tran = 0
	ELSE
	 BEGIN
		BEGIN TRANSACTION
		SET @local_tran = 1		
	 END

	IF @WorkflowInstanceId IS NULL AND @WorkflowInstanceInternalId IS NULL
	 BEGIN
		SELECT @error_desc = @localized_string_InsertActivityExecutionStatusEvent_Failed_InvalidParam
		GOTO FAILED
	 END

	DECLARE @InternalId bigint

	SELECT @InternalId = @WorkflowInstanceInternalId

	IF @InternalId IS NULL
	 BEGIN
		exec @ret = [dbo].[GetWorkflowInstanceInternalId]	@WorkflowInstanceId				= @WorkflowInstanceId
															,@ContextGuid					= @WorkflowInstanceContextGuid
															,@WorkflowInstanceInternalId	= @InternalId OUTPUT

		IF @ret IS NULL OR @ret <> 0 OR @InternalId IS NULL OR @InternalId <= 0
		 BEGIN
			SELECT @error_desc = @localized_string_InsertActivityExecutionStatusEvent_Failed_WorkflowInstanceInternalId
			GOTO FAILED
		 END
	 END
	
	DECLARE @ActivityInstanceId					bigint
			,@QualifiedName						nvarchar(128)
			,@ContextGuid						uniqueidentifier
			,@ParentContextGuid					uniqueidentifier
			,@ExecutionStatusId					tinyint		
			,@EventDateTime						datetime	
			,@EventOrder						int
			,@ActivityExecutionStatusEventId	bigint
			,@iteration							smallint

	SELECT	@ActivityInstanceId					= @ActivityInstanceId1
			,@QualifiedName						= @QualifiedName1
			,@ContextGuid						= @ContextGuid1
			,@ParentContextGuid					= @ParentContextGuid1
			,@ExecutionStatusId					= @ExecutionStatusId1		
			,@EventDateTime						= @EventDateTime1	
			,@EventOrder						= @EventOrder1
			,@iteration							= 1
	
	WHILE @QualifiedName IS NOT NULL
	 BEGIN
		-- This select->insert sequence is OK because workflows are single threaded
		-- a record isn't going to sneak in between the select and insert
		IF @ActivityInstanceId IS NULL
		 BEGIN
			EXEC @ret = [dbo].[GetActivityInstanceId]		@WorkflowInstanceInternalId			= @InternalId 
															,@QualifiedName						= @QualifiedName
															,@ContextGuid						= @ContextGuid
															,@ParentContextGuid					= @ParentContextGuid
															,@ActivityInstanceId				= @ActivityInstanceId OUTPUT
		
			SELECT @error = @@ERROR
			IF @error IS NULL OR @error <> 0 OR @ret IS NULL OR @ret <> 0 OR @ActivityInstanceId IS NULL OR @ActivityInstanceId <= 0
			 BEGIN
				SELECT @error_desc = @localized_string_InsertActivityExecutionStatusEvent_Failed_ActivityInstanceIdSel
				GOTO FAILED
			 END
		 END
		/*
			Insert this event in activity status
		*/
		INSERT [dbo].[ActivityExecutionStatusEvent] (
				[WorkflowInstanceInternalId]
				,[EventOrder]				
				,[ActivityInstanceId]		
				,[ExecutionStatusId]					
				,[EventDateTime]			
		) VALUES (
				@InternalId
				,@EventOrder
				,@ActivityInstanceId
				,@ExecutionStatusId
				,@EventDateTime
		)

		SELECT	@error							= @@ERROR
				,@ActivityExecutionStatusEventId= scope_identity()
				,@WorkflowInstanceInternalId	= @InternalId

		IF @error <> 0
		 BEGIN
			SELECT @error_desc = @localized_string_InsertActivityExecutionStatusEvent_Failed_ActivityStatusInsert
			GOTO FAILED
		 END

		IF @ActivityExecutionStatusEventId IS NULL OR @ActivityExecutionStatusEventId < 0
		 BEGIN
			SELECT @error_desc = @localized_string_InsertActivityExecutionStatusEvent_Failed_NoEventId
			GOTO FAILED
		 END

		IF @iteration = 1
		 BEGIN
			/*
				Set output parameters
			*/
			SELECT	@ActivityInstanceId1				= @ActivityInstanceId
					,@ActivityExecutionStatusEventId1	= @ActivityExecutionStatusEventId

			SELECT @ActivityInstanceId = NULL

			IF @ActivityInstanceId2 IS NOT NULL
			 BEGIN
				/*
					Id was cached in the tracking channel and passed in
				*/
				SELECT @ActivityInstanceId				= @ActivityInstanceId2
			 END
			ELSE
			 BEGIN
				/*
					If the IDs of the next activity match the IDs of a previous activity re-use the ActivityInstanceId value
				*/
				IF @QualifiedName2 = @QualifiedName1 AND @ContextGuid2 = @ContextGuid1 AND @ParentContextGuid2 = @ParentContextGuid1
				 BEGIN
					SELECT @ActivityInstanceId				= @ActivityInstanceId1
				 END
			 END

			SELECT	@QualifiedName						= @QualifiedName2
					,@ContextGuid						= @ContextGuid2
					,@ParentContextGuid					= @ParentContextGuid2
					,@ExecutionStatusId					= @ExecutionStatusId2		
					,@EventDateTime						= @EventDateTime2
					,@EventOrder						= @EventOrder2
					,@iteration							= 2
		 END
		ELSE IF @iteration = 2
		 BEGIN
			/*
				Set output parameters
			*/
			SELECT	@ActivityInstanceId2				= @ActivityInstanceId
					,@ActivityExecutionStatusEventId2	= @ActivityExecutionStatusEventId

			SELECT @ActivityInstanceId = NULL
			IF @ActivityInstanceId3 IS NOT NULL
			 BEGIN
				/*
					Id was cached in the tracking channel and passed in
				*/
				SELECT @ActivityInstanceId				= @ActivityInstanceId3
			 END
			ELSE
			 BEGIN
				/*
					If the IDs of the next activity match the IDs of a previous activity re-use the ActivityInstanceId value
				*/
				IF @QualifiedName3 = @QualifiedName1 AND @ContextGuid3 = @ContextGuid1 AND @ParentContextGuid3 = @ParentContextGuid1
				 BEGIN
					SELECT @ActivityInstanceId				= @ActivityInstanceId1
				 END
				ELSE IF @QualifiedName3 = @QualifiedName2 AND @ContextGuid3 = @ContextGuid2 AND @ParentContextGuid3 = @ParentContextGuid2
				 BEGIN
					SELECT @ActivityInstanceId				= @ActivityInstanceId2
				 END	
			 END

			SELECT	@QualifiedName						= @QualifiedName3
					,@ContextGuid						= @ContextGuid3
					,@ParentContextGuid					= @ParentContextGuid3
					,@ExecutionStatusId					= @ExecutionStatusId3		
					,@EventDateTime						= @EventDateTime3
					,@EventOrder						= @EventOrder3
					,@iteration							= 3		
		 END
		ELSE IF @iteration = 3
		 BEGIN
			/*
				Set output parameters
			*/
			SELECT	@ActivityInstanceId3				= @ActivityInstanceId
					,@ActivityExecutionStatusEventId3	= @ActivityExecutionStatusEventId

			SELECT @ActivityInstanceId = NULL

			IF @ActivityInstanceId4 IS NOT NULL
			 BEGIN
				/*
					Id was cached in the tracking channel and passed in
				*/
				SELECT @ActivityInstanceId				= @ActivityInstanceId4
			 END
			ELSE
			 BEGIN
				/*
					If the IDs of the next activity match the IDs of a previous activity re-use the ActivityInstanceId value
				*/
				IF @QualifiedName4 = @QualifiedName1 AND @ContextGuid4 = @ContextGuid1 AND @ParentContextGuid4 = @ParentContextGuid1
				 BEGIN
					SELECT @ActivityInstanceId				= @ActivityInstanceId1
				 END
				ELSE IF @QualifiedName4 = @QualifiedName2 AND @ContextGuid4 = @ContextGuid2 AND @ParentContextGuid4 = @ParentContextGuid2
				 BEGIN
					SELECT @ActivityInstanceId				= @ActivityInstanceId2
				 END
				ELSE IF @QualifiedName4 = @QualifiedName3 AND @ContextGuid4 = @ContextGuid3 AND @ParentContextGuid4 = @ParentContextGuid3
				 BEGIN
					SELECT @ActivityInstanceId				= @ActivityInstanceId3
				 END
			 END

			SELECT	@QualifiedName						= @QualifiedName4
					,@ContextGuid						= @ContextGuid4
					,@ParentContextGuid					= @ParentContextGuid4
					,@ExecutionStatusId					= @ExecutionStatusId4		
					,@EventDateTime						= @EventDateTime4
					,@EventOrder						= @EventOrder4
					,@iteration							= 4	
		 END
		ELSE IF @iteration = 4
		 BEGIN	
			/*
				Set output parameters
			*/
			SELECT	@ActivityInstanceId4				= @ActivityInstanceId
					,@ActivityExecutionStatusEventId4	= @ActivityExecutionStatusEventId

			SELECT @ActivityInstanceId = NULL

			IF @ActivityInstanceId5 IS NOT NULL
			 BEGIN
				/*
					Id was cached in the tracking channel and passed in
				*/
				SELECT @ActivityInstanceId				= @ActivityInstanceId5
			 END
			ELSE
			 BEGIN
				/*
					If the IDs of the next activity match the IDs of a previous activity re-use the ActivityInstanceId value
				*/
				IF @QualifiedName5 = @QualifiedName1 AND @ContextGuid5 = @ContextGuid1 AND @ParentContextGuid5 = @ParentContextGuid1
				 BEGIN
					SELECT @ActivityInstanceId				= @ActivityInstanceId1
				 END
				ELSE IF @QualifiedName5 = @QualifiedName2 AND @ContextGuid5 = @ContextGuid2 AND @ParentContextGuid5 = @ParentContextGuid2
				 BEGIN
					SELECT @ActivityInstanceId				= @ActivityInstanceId2
				 END
				ELSE IF @QualifiedName5 = @QualifiedName3 AND @ContextGuid5 = @ContextGuid3 AND @ParentContextGuid5 = @ParentContextGuid3
				 BEGIN
					SELECT @ActivityInstanceId				= @ActivityInstanceId3
				 END
				ELSE IF @QualifiedName5 = @QualifiedName4 AND @ContextGuid5 = @ContextGuid4 AND @ParentContextGuid5 = @ParentContextGuid4
				 BEGIN
					SELECT @ActivityInstanceId				= @ActivityInstanceId4
				 END
			 END

			SELECT	@QualifiedName						= @QualifiedName5
					,@ContextGuid						= @ContextGuid5
					,@ParentContextGuid					= @ParentContextGuid5
					,@ExecutionStatusId					= @ExecutionStatusId5		
					,@EventDateTime						= @EventDateTime5
					,@EventOrder						= @EventOrder5
					,@iteration							= 5	
		 END
		ELSE IF @iteration = 5
		 BEGIN
			SELECT	@ActivityInstanceId5				= @ActivityInstanceId
					,@ActivityExecutionStatusEventId5	= @ActivityExecutionStatusEventId -- set the output id param for this event

			BREAK
		 END
	 END
	

	IF @local_tran = 1
		COMMIT TRANSACTION

	SET @ret = 0
	GOTO DONE

FAILED:
	IF @local_tran = 1
		ROLLBACK TRANSACTION

	RAISERROR( @error_desc, 16, -1 )

	SET @ret = -1
	GOTO DONE

DONE:
	RETURN @ret

 END
