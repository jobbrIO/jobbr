﻿SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Jobs](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UniqueName] [varchar](100) NULL,
	[Title] [varchar](100) NULL,
	[Parameters] [varchar](max) NULL,
	[Type] [varchar](100) NULL,
	[UpdatedDateTimeUtc] [datetime] NULL,
	[CreatedDateTimeUtc] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[UniqueName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [dbo].[Triggers](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Type] [varchar](255) NOT NULL,
	[JobId] [bigint] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[UserId] [varchar](100) NULL,
	[UserDisplayName] [varchar](100) NULL,
	[Parameters] [varchar](max) NULL,
	[Comment] [varchar](8000) NULL,
	[CreatedDateTimeUtc] [datetime] NOT NULL,
	[StartDateTimeUtc] [datetime] NULL,
	[EndDateTimeUtc] [datetime] NULL,
	[Definition] [varchar](8000) NULL,
	[NoParallelExecution] [bit] NOT NULL,
	[DelayedMinutes] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


CREATE TABLE [dbo].[JobRuns](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[JobId] [bigint] NOT NULL,
	[TriggerId] [bigint] NOT NULL,
	[State] [varchar](255) NOT NULL,
	[Progress] [float] NULL,
	[PlannedStartDateTimeUtc] [datetime] NOT NULL,
	[ActualStartDateTimeUtc] [datetime] NULL,
	[ActualEndDateTimeUtc] [datetime] NULL,
	[EstimatedEndDateTimeUtc] [datetime] NULL,
	[JobParameters] [varchar](max) NULL,
	[InstanceParameters] [varchar](max) NULL,
	[Pid] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


